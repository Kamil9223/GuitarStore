using Auth.Core.Authorization;
using Auth.Core.Entities;
using Auth.Core.Events.Outgoing;
using Common.RabbitMq.Abstractions.EventHandlers;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Services;

internal sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IIntegrationEventPublisher integrationEventPublisher) : IAuthService
{
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
            return new AuthLoginResult(AuthLoginStatus.Succeeded);
        }

        if (result.IsLockedOut)
        {
            return new AuthLoginResult(AuthLoginStatus.LockedOut);
        }

        if (result.IsNotAllowed)
        {
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

        await signInManager.SignInAsync(user, isPersistent: false);
        return AuthRegisterResult.Success();
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
