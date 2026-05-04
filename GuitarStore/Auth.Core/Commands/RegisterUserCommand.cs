using Application.CQRS.Command;
using Auth.Core.Authorization;
using Auth.Core.Data;
using Auth.Core.Entities;
using Auth.Core.Events.Outgoing;
using Auth.Core.Outbox;
using Auth.Core.Services;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Auth.Core.Commands;

public sealed record RegisterUserCommand(
    string Name,
    string LastName,
    string Email,
    string Password) : ICommand;

public enum AuthRegisterStatus
{
    Succeeded,
    PendingEmailConfirmation,
    DuplicateEmail,
    Failed
}

public sealed record AuthRegisterResult(AuthRegisterStatus Status, IReadOnlyCollection<string> Errors)
{
    public bool Succeeded => Status is AuthRegisterStatus.Succeeded or AuthRegisterStatus.PendingEmailConfirmation;
    public bool RequiresEmailConfirmation => Status == AuthRegisterStatus.PendingEmailConfirmation;

    public static AuthRegisterResult Success() => new(AuthRegisterStatus.Succeeded, []);
    public static AuthRegisterResult PendingEmailConfirmation() => new(AuthRegisterStatus.PendingEmailConfirmation, []);
    public static AuthRegisterResult DuplicateEmail() => new(AuthRegisterStatus.DuplicateEmail, []);
    public static AuthRegisterResult Failed(params string[] errors) => new(AuthRegisterStatus.Failed, errors);
    public static AuthRegisterResult Failed(IEnumerable<string> errors) => new(AuthRegisterStatus.Failed, errors.ToArray());
}

internal sealed class RegisterUserCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IAuthOutboxPublisher authOutboxPublisher,
    AuthDbContext authDbContext,
    IAuthEmailSender authEmailSender,
    IAuthAccountLinkFactory authAccountLinkFactory,
    IOptions<Configuration.AuthOptions> authOptions) : ICommandHandler<AuthRegisterResult, RegisterUserCommand>
{
    private readonly bool _requireEmailConfirmed = authOptions.Value.RequireEmailConfirmed;

    public async Task<AuthRegisterResult> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(command.Email) is not null)
            return AuthRegisterResult.DuplicateEmail();

        var user = new User
        {
            Id = AuthId.New(),
            UserName = command.Email,
            Email = command.Email
        };

        await using var tx = await authDbContext.Database.BeginTransactionAsync(ct);

        var createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            await tx.RollbackAsync(ct);
            return AuthRegisterResult.Failed(createResult.Errors.Select(static e => e.Description));
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, AuthRoles.User);
        if (!addToRoleResult.Succeeded)
        {
            await tx.RollbackAsync(ct);
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

        await authDbContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        if (_requireEmailConfirmed)
            return AuthRegisterResult.PendingEmailConfirmation();

        await signInManager.SignInAsync(user, isPersistent: false);
        return AuthRegisterResult.Success();
    }
}
