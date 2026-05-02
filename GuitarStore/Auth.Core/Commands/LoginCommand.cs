using Application.CQRS.Command;
using Auth.Core.Configuration;
using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Auth.Core.Commands;

public sealed record LoginCommand(
    string EmailOrUserName,
    string Password,
    bool RememberMe) : ICommand;

public enum AuthLoginStatus
{
    Succeeded,
    RequiresPasswordChange,
    RequiresEmailConfirmation,
    InvalidCredentials,
    LockedOut,
    NotAllowed
}

public sealed record AuthLoginResult(AuthLoginStatus Status)
{
    public bool Succeeded => Status == AuthLoginStatus.Succeeded;
}

internal sealed class LoginCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IOptions<AuthOptions> authOptions)
    : ICommandHandler<AuthLoginResult, LoginCommand>
{
    public async Task<AuthLoginResult> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.EmailOrUserName)
                   ?? await userManager.FindByNameAsync(command.EmailOrUserName);

        if (user is null)
            return new AuthLoginResult(AuthLoginStatus.InvalidCredentials);
        
        if (authOptions.Value.RequireEmailConfirmed && !user.EmailConfirmed)
            return new AuthLoginResult(AuthLoginStatus.RequiresEmailConfirmation);

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            command.Password,
            command.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return user.MustChangePassword
                ? new AuthLoginResult(AuthLoginStatus.RequiresPasswordChange)
                : new AuthLoginResult(AuthLoginStatus.Succeeded);
        }

        if (result.IsLockedOut)
            return new AuthLoginResult(AuthLoginStatus.LockedOut);

        if (result.IsNotAllowed)
        {
            if (!await userManager.IsEmailConfirmedAsync(user))
                return new AuthLoginResult(AuthLoginStatus.RequiresEmailConfirmation);

            return new AuthLoginResult(AuthLoginStatus.NotAllowed);
        }

        return new AuthLoginResult(AuthLoginStatus.InvalidCredentials);
    }
}