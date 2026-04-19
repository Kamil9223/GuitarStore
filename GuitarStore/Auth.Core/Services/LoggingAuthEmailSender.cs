using Microsoft.Extensions.Logging;

namespace Auth.Core.Services;

internal sealed class LoggingAuthEmailSender(ILogger<LoggingAuthEmailSender> logger) : IAuthEmailSender
{
    public Task SendEmailConfirmationAsync(EmailConfirmationMessage message, CancellationToken ct)
    {
        logger.LogInformation(
            "Auth email confirmation requested for {Email}. Confirmation link: {ConfirmationLink}",
            message.Email,
            message.ConfirmationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(PasswordResetMessage message, CancellationToken ct)
    {
        logger.LogInformation(
            "Auth password reset requested for {Email}. Reset link: {ResetLink}",
            message.Email,
            message.ResetLink);

        return Task.CompletedTask;
    }
}
