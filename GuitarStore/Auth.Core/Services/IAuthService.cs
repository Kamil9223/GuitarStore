using System.Security.Claims;

namespace Auth.Core.Services;

public interface IAuthService
{
    Task<AuthLoginResult> LoginAsync(AuthLoginRequest request);
    Task<AuthRegisterResult> RegisterAsync(AuthRegisterRequest request, CancellationToken ct);
    Task<AuthConfirmEmailResult> ConfirmEmailAsync(string userId, string encodedToken);
    Task ForgotPasswordAsync(AuthForgotPasswordRequest request, CancellationToken ct);
    Task<AuthResetPasswordResult> ResetPasswordAsync(AuthResetPasswordRequest request);
    Task<bool> RequiresPasswordChangeAsync(ClaimsPrincipal principal);
    Task<AuthChangePasswordResult> ChangePasswordAsync(ClaimsPrincipal principal, AuthChangePasswordRequest request);
    Task LogoutAsync();
}

public sealed record AuthLoginRequest(
    string EmailOrUserName,
    string Password,
    bool RememberMe);

public sealed record AuthRegisterRequest(
    string Name,
    string LastName,
    string Email,
    string Password);

public enum AuthLoginStatus
{
    Succeeded,
    RequiresPasswordChange,
    RequiresEmailConfirmation,
    InvalidCredentials,
    LockedOut,
    NotAllowed
}

public sealed record AuthLoginResult(AuthLoginStatus Status)
{
    public bool Succeeded => Status == AuthLoginStatus.Succeeded;
}

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

public enum AuthConfirmEmailStatus
{
    Succeeded,
    AlreadyConfirmed,
    InvalidTokenOrUser
}

public sealed record AuthConfirmEmailResult(AuthConfirmEmailStatus Status)
{
    public bool Succeeded => Status is AuthConfirmEmailStatus.Succeeded or AuthConfirmEmailStatus.AlreadyConfirmed;

    public static AuthConfirmEmailResult Success() => new(AuthConfirmEmailStatus.Succeeded);
    public static AuthConfirmEmailResult AlreadyConfirmed() => new(AuthConfirmEmailStatus.AlreadyConfirmed);
    public static AuthConfirmEmailResult InvalidTokenOrUser() => new(AuthConfirmEmailStatus.InvalidTokenOrUser);
}

public sealed record AuthForgotPasswordRequest(string Email);

public sealed record AuthResetPasswordRequest(
    string UserId,
    string EncodedToken,
    string NewPassword);

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

public sealed record AuthChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public enum AuthChangePasswordStatus
{
    Succeeded,
    CurrentUserNotFound,
    PasswordChangeNotRequired,
    Failed
}

public sealed record AuthChangePasswordResult(AuthChangePasswordStatus Status, IReadOnlyCollection<string> Errors)
{
    public bool Succeeded => Status == AuthChangePasswordStatus.Succeeded;

    public static AuthChangePasswordResult Success() => new(AuthChangePasswordStatus.Succeeded, []);
    public static AuthChangePasswordResult CurrentUserNotFoundResult() => new(AuthChangePasswordStatus.CurrentUserNotFound, []);
    public static AuthChangePasswordResult PasswordChangeNotRequiredResult() => new(AuthChangePasswordStatus.PasswordChangeNotRequired, []);
    public static AuthChangePasswordResult Failed(IEnumerable<string> errors) => new(AuthChangePasswordStatus.Failed, errors.ToArray());
}
