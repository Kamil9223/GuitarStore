namespace Auth.Core.Services;

public interface IAuthEmailSender
{
    Task SendEmailConfirmationAsync(EmailConfirmationMessage message, CancellationToken ct);
    Task SendPasswordResetAsync(PasswordResetMessage message, CancellationToken ct);
}

public sealed record EmailConfirmationMessage(string Email, Uri ConfirmationLink);

public sealed record PasswordResetMessage(string Email, Uri ResetLink);
