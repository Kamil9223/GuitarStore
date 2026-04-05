namespace Auth.Core.Services;

public interface IAuthService
{
    Task<AuthLoginResult> LoginAsync(AuthLoginRequest request);
    Task<AuthRegisterResult> RegisterAsync(AuthRegisterRequest request, CancellationToken ct);
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
    DuplicateEmail,
    Failed
}

public sealed record AuthRegisterResult(AuthRegisterStatus Status, IReadOnlyCollection<string> Errors)
{
    public bool Succeeded => Status == AuthRegisterStatus.Succeeded;

    public static AuthRegisterResult Success() => new(AuthRegisterStatus.Succeeded, []);
    public static AuthRegisterResult DuplicateEmail() => new(AuthRegisterStatus.DuplicateEmail, []);
    public static AuthRegisterResult Failed(params string[] errors) => new(AuthRegisterStatus.Failed, errors);
    public static AuthRegisterResult Failed(IEnumerable<string> errors) => new(AuthRegisterStatus.Failed, errors.ToArray());
}
