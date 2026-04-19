using Auth.Core.Authorization;
using Auth.Core.Configuration;
using Auth.Core.Entities;
using Auth.Core.Events.Outgoing;
using Common.RabbitMq.Abstractions.EventHandlers;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Auth.Core.Services;

internal sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IIntegrationEventPublisher integrationEventPublisher,
    IAuthEmailSender authEmailSender,
    IAuthAccountLinkFactory authAccountLinkFactory,
    IOptions<AuthOptions> authOptions) : IAuthService
{
    private readonly bool _requireEmailConfirmed = authOptions.Value.RequireEmailConfirmed;

    public async Task<AuthLoginResult> LoginAsync(AuthLoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.EmailOrUserName)
            ?? await userManager.FindByNameAsync(request.EmailOrUserName);

        if (user is null)
        {
            return new AuthLoginResult(AuthLoginStatus.InvalidCredentials);
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (_requireEmailConfirmed && !user.EmailConfirmed)
            {
                await signInManager.SignOutAsync();
                return new AuthLoginResult(AuthLoginStatus.RequiresEmailConfirmation);
            }

            if (user.MustChangePassword)
            {
                return new AuthLoginResult(AuthLoginStatus.RequiresPasswordChange);
            }

            return new AuthLoginResult(AuthLoginStatus.Succeeded);
        }

        if (result.IsLockedOut)
        {
            return new AuthLoginResult(AuthLoginStatus.LockedOut);
        }

        if (result.IsNotAllowed)
        {
            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return new AuthLoginResult(AuthLoginStatus.RequiresEmailConfirmation);
            }

            return new AuthLoginResult(AuthLoginStatus.NotAllowed);
        }

        return new AuthLoginResult(AuthLoginStatus.InvalidCredentials);
    }

    public async Task<AuthRegisterResult> RegisterAsync(AuthRegisterRequest request, CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return AuthRegisterResult.DuplicateEmail();
        }

        var user = new User
        {
            Id = AuthId.New(),
            UserName = request.Email,
            Email = request.Email
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AuthRegisterResult.Failed(createResult.Errors.Select(static error => error.Description));
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, AuthRoles.User);
        if (!addToRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return AuthRegisterResult.Failed(addToRoleResult.Errors.Select(static error => error.Description));
        }

        try
        {
            if (_requireEmailConfirmed)
            {
                var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = authAccountLinkFactory.CreateEmailConfirmationLink(user, confirmationToken);

                await authEmailSender.SendEmailConfirmationAsync(
                    new EmailConfirmationMessage(request.Email, confirmationLink),
                    ct);
            }

            var userRegisteredEvent = new UserRegisteredEvent(
                UserId: user.Id.Value,
                Email: request.Email,
                Name: request.Name,
                LastName: request.LastName,
                OccurredAtUtc: DateTimeOffset.UtcNow);

            await integrationEventPublisher.Publish(userRegisteredEvent, ct);
        }
        catch
        {
            var errors = new List<string>();
            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                errors.AddRange(deleteResult.Errors.Select(static error => error.Description));
            }

            errors.Add("Registration could not be completed. Please try again.");
            return AuthRegisterResult.Failed(errors);
        }

        if (_requireEmailConfirmed)
        {
            return AuthRegisterResult.PendingEmailConfirmation();
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return AuthRegisterResult.Success();
    }

    public async Task<AuthConfirmEmailResult> ConfirmEmailAsync(string userId, string encodedToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AuthConfirmEmailResult.InvalidTokenOrUser();
        }

        if (user.EmailConfirmed)
        {
            return AuthConfirmEmailResult.AlreadyConfirmed();
        }

        string token;
        try
        {
            token = authAccountLinkFactory.DecodeToken(encodedToken);
        }
        catch (FormatException)
        {
            return AuthConfirmEmailResult.InvalidTokenOrUser();
        }

        var confirmResult = await userManager.ConfirmEmailAsync(user, token);
        if (!confirmResult.Succeeded)
        {
            return AuthConfirmEmailResult.InvalidTokenOrUser();
        }

        return AuthConfirmEmailResult.Success();
    }

    public async Task ForgotPasswordAsync(AuthForgotPasswordRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.EmailConfirmed)
        {
            return;
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = authAccountLinkFactory.CreatePasswordResetLink(user, resetToken);

        await authEmailSender.SendPasswordResetAsync(
            new PasswordResetMessage(user.Email!, resetLink),
            ct);
    }

    public async Task<AuthResetPasswordResult> ResetPasswordAsync(AuthResetPasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return AuthResetPasswordResult.InvalidTokenOrUser();
        }

        string token;
        try
        {
            token = authAccountLinkFactory.DecodeToken(request.EncodedToken);
        }
        catch (FormatException)
        {
            return AuthResetPasswordResult.InvalidTokenOrUser();
        }

        var resetPasswordResult = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            return resetPasswordResult.Errors.Any(static error => error.Code == "InvalidToken")
                ? AuthResetPasswordResult.InvalidTokenOrUser()
                : AuthResetPasswordResult.Failed(resetPasswordResult.Errors.Select(static error => error.Description));
        }

        return AuthResetPasswordResult.Success();
    }

    public async Task<bool> RequiresPasswordChangeAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        return user?.MustChangePassword == true;
    }

    public async Task<AuthChangePasswordResult> ChangePasswordAsync(ClaimsPrincipal principal, AuthChangePasswordRequest request)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return AuthChangePasswordResult.CurrentUserNotFoundResult();
        }

        if (!user.MustChangePassword)
        {
            return AuthChangePasswordResult.PasswordChangeNotRequiredResult();
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            return AuthChangePasswordResult.Failed(changePasswordResult.Errors.Select(static error => error.Description));
        }

        user.MarkPasswordChanged();
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return AuthChangePasswordResult.Failed(updateResult.Errors.Select(static error => error.Description));
        }

        await signInManager.RefreshSignInAsync(user);
        return AuthChangePasswordResult.Success();
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
