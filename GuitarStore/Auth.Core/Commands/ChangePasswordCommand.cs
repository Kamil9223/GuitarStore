using Application.CQRS.Command;
using Auth.Core.Entities;
using Auth.Core.Services;
using Common.Errors.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth.Core.Commands;

internal sealed record ChangePasswordCommand(
    ClaimsPrincipal Principal,
    string CurrentPassword,
    string NewPassword) : ICommand;

internal sealed class ChangePasswordCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager) : ICommandHandler<AuthChangePasswordResult, ChangePasswordCommand>
{
    public async Task<AuthChangePasswordResult> Handle(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(command.Principal)
            ?? throw new InvalidOperationException("Current user was not found during password change.");

        if (!user.MustChangePassword)
        {
            throw new DomainException("Password change is not required for this account.");
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            return AuthChangePasswordResult.Failed(changePasswordResult.Errors.Select(static e => e.Description));
        }

        user.MarkPasswordChanged();
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return AuthChangePasswordResult.Failed(updateResult.Errors.Select(static e => e.Description));
        }

        await signInManager.RefreshSignInAsync(user);
        return AuthChangePasswordResult.Success();
    }
}
