using Application.CQRS.Command;
using Application.CQRS.Query;
using Auth.Core.Commands;
using Auth.Core.Configuration;
using Auth.Core.Entities;
using Auth.Core.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Auth.Core.Services;

internal sealed class AccountService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ICommandHandler<AuthRegisterResult, RegisterUserCommand> registerUserCommandHandler,
    ICommandHandler<AuthConfirmEmailResult, ConfirmEmailCommand> confirmEmailCommandHandler,
    ICommandHandler<RequestPasswordResetCommand> requestPasswordResetCommandHandler,
    ICommandHandler<AuthResetPasswordResult, ResetPasswordCommand> resetPasswordCommandHandler,
    ICommandHandler<AuthChangePasswordResult, ChangePasswordCommand> changePasswordCommandHandler,
    IQueryHandler<RequiresPasswordChangeQuery, RequiresPasswordChangeQueryResult> requiresPasswordChangeQueryHandler,
    IOptions<AuthOptions> authOptions) : IAccountService
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

    public Task<AuthRegisterResult> RegisterAsync(AuthRegisterRequest request, CancellationToken ct) =>
        registerUserCommandHandler.Handle(
            new RegisterUserCommand(request.Name, request.LastName, request.Email, request.Password), ct);

    public Task<AuthConfirmEmailResult> ConfirmEmailAsync(string userId, string encodedToken) =>
        confirmEmailCommandHandler.Handle(
            new ConfirmEmailCommand(userId, encodedToken), CancellationToken.None);

    public Task ForgotPasswordAsync(AuthForgotPasswordRequest request, CancellationToken ct) =>
        requestPasswordResetCommandHandler.Handle(
            new RequestPasswordResetCommand(request.Email), ct);

    public Task<AuthResetPasswordResult> ResetPasswordAsync(AuthResetPasswordRequest request) =>
        resetPasswordCommandHandler.Handle(
            new ResetPasswordCommand(request.UserId, request.EncodedToken, request.NewPassword), CancellationToken.None);

    public async Task<bool> RequiresPasswordChangeAsync(ClaimsPrincipal principal)
    {
        var result = await requiresPasswordChangeQueryHandler.Handle(
            new RequiresPasswordChangeQuery(principal), CancellationToken.None);
        return result.IsRequired;
    }

    public Task<AuthChangePasswordResult> ChangePasswordAsync(ClaimsPrincipal principal, AuthChangePasswordRequest request) =>
        changePasswordCommandHandler.Handle(
            new ChangePasswordCommand(principal, request.CurrentPassword, request.NewPassword), CancellationToken.None);

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
