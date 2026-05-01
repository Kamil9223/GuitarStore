using Application.CQRS.Command;
using Auth.Core.Entities;
using Auth.Core.Services;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Commands;

internal sealed record ConfirmEmailCommand(string UserId, string EncodedToken) : ICommand;

internal sealed class ConfirmEmailCommandHandler(
    UserManager<User> userManager,
    IAuthAccountLinkFactory authAccountLinkFactory) : ICommandHandler<AuthConfirmEmailResult, ConfirmEmailCommand>
{
    public async Task<AuthConfirmEmailResult> Handle(ConfirmEmailCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
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
            token = authAccountLinkFactory.DecodeToken(command.EncodedToken);
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
}
