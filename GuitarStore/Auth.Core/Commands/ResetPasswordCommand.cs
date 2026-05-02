using Application.CQRS.Command;
using Auth.Core.Entities;
using Auth.Core.Services;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Commands;

public sealed record ResetPasswordCommand(
    string UserId,
    string EncodedToken,
    string NewPassword) : ICommand;

public enum AuthResetPasswordStatus
{
    Succeeded,
    InvalidTokenOrUser,
    Failed
}

public sealed record AuthResetPasswordResult(AuthResetPasswordStatus Status, IReadOnlyCollection<string> Errors)
{
    public bool Succeeded => Status == AuthResetPasswordStatus.Succeeded;

    public static AuthResetPasswordResult Success() => new(AuthResetPasswordStatus.Succeeded, []);
    public static AuthResetPasswordResult InvalidTokenOrUser() => new(AuthResetPasswordStatus.InvalidTokenOrUser, []);
    public static AuthResetPasswordResult Failed(IEnumerable<string> errors) => new(AuthResetPasswordStatus.Failed, errors.ToArray());
}

internal sealed class ResetPasswordCommandHandler(
    UserManager<User> userManager,
    IAuthAccountLinkFactory authAccountLinkFactory) : ICommandHandler<AuthResetPasswordResult, ResetPasswordCommand>
{
    public async Task<AuthResetPasswordResult> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return AuthResetPasswordResult.InvalidTokenOrUser();

        string token;
        try
        {
            token = authAccountLinkFactory.DecodeToken(command.EncodedToken);
        }
        catch (FormatException)
        {
            return AuthResetPasswordResult.InvalidTokenOrUser();
        }

        var resetPasswordResult = await userManager.ResetPasswordAsync(user, token, command.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            return resetPasswordResult.Errors.Any(static e => e.Code == "InvalidToken")
                ? AuthResetPasswordResult.InvalidTokenOrUser()
                : AuthResetPasswordResult.Failed(resetPasswordResult.Errors.Select(static e => e.Description));
        }

        return AuthResetPasswordResult.Success();
    }
}
