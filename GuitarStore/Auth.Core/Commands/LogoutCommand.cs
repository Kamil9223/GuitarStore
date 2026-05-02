using Application.CQRS.Command;
using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Commands;

public sealed class LogoutCommand : ICommand;

internal sealed class LogoutCommandHandler(
    SignInManager<User> signInManager) : ICommandHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        return signInManager.SignOutAsync();
    }
}