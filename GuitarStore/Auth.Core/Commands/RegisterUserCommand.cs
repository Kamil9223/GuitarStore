using Application.CQRS.Command;
using Auth.Core.Authorization;
using Auth.Core.Entities;
using Auth.Core.Events.Outgoing;
using Auth.Core.Services;
using Common.Outbox;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Auth.Core.Commands;

internal sealed record RegisterUserCommand(
    string Name,
    string LastName,
    string Email,
    string Password) : ICommand;

internal sealed class RegisterUserCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IOutboxEventPublisher authOutboxPublisher,
    IAuthEmailSender authEmailSender,
    IAuthAccountLinkFactory authAccountLinkFactory,
    IOptions<Configuration.AuthOptions> authOptions) : ICommandHandler<AuthRegisterResult, RegisterUserCommand>
{
    private readonly bool _requireEmailConfirmed = authOptions.Value.RequireEmailConfirmed;

    public async Task<AuthRegisterResult> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(command.Email) is not null)
        {
            return AuthRegisterResult.DuplicateEmail();
        }

        var user = new User
        {
            Id = AuthId.New(),
            UserName = command.Email,
            Email = command.Email
        };

        var createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            return AuthRegisterResult.Failed(createResult.Errors.Select(static e => e.Description));
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, AuthRoles.User);
        if (!addToRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return AuthRegisterResult.Failed(addToRoleResult.Errors.Select(static e => e.Description));
        }

        if (_requireEmailConfirmed)
        {
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = authAccountLinkFactory.CreateEmailConfirmationLink(user, confirmationToken);
            await authEmailSender.SendEmailConfirmationAsync(
                new EmailConfirmationMessage(command.Email, confirmationLink), ct);
        }

        await authOutboxPublisher.PublishToOutbox(new UserRegisteredEvent(
            UserId: user.Id.Value,
            Email: command.Email,
            Name: command.Name,
            LastName: command.LastName,
            OccurredAtUtc: DateTimeOffset.UtcNow), ct);

        if (_requireEmailConfirmed)
        {
            return AuthRegisterResult.PendingEmailConfirmation();
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return AuthRegisterResult.Success();
    }
}
