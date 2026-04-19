using Auth.Core.Services;

namespace Tests.EndToEnd.Setup.Modules.Auth;

internal sealed class TestAuthEmailSender : IAuthEmailSender
{
    private readonly object _sync = new();
    private readonly List<EmailConfirmationMessage> _emailConfirmations = [];
    private readonly List<PasswordResetMessage> _passwordResets = [];

    public Task SendEmailConfirmationAsync(EmailConfirmationMessage message, CancellationToken ct)
    {
        lock (_sync)
        {
            _emailConfirmations.Add(message);
        }

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(PasswordResetMessage message, CancellationToken ct)
    {
        lock (_sync)
        {
            _passwordResets.Add(message);
        }

        return Task.CompletedTask;
    }

    public EmailConfirmationMessage? FindLatestEmailConfirmation(string email)
    {
        lock (_sync)
        {
            return _emailConfirmations.LastOrDefault(
                x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public PasswordResetMessage? FindLatestPasswordReset(string email)
    {
        lock (_sync)
        {
            return _passwordResets.LastOrDefault(
                x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
