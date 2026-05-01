using Application.CQRS.Command;
using Auth.Core.Entities;
using Auth.Core.Services;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Commands;

internal sealed record RequestPasswordResetCommand(string Email) : ICommand;

internal sealed class RequestPasswordResetCommandHandler(
    UserManager<User> userManager,
    IAuthEmailSender authEmailSender,
    IAuthAccountLinkFactory authAccountLinkFactory) : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task Handle(RequestPasswordResetCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || !user.EmailConfirmed)
        {
            return;
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = authAccountLinkFactory.CreatePasswordResetLink(user, resetToken);

        await authEmailSender.SendPasswordResetAsync(
            new PasswordResetMessage(user.Email!, resetLink), ct);
    }
}
