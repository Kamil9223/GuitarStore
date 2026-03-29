using System.Security.Cryptography.X509Certificates;

namespace Auth.Core.Configuration;

public sealed record AuthOptions
{
    public const string SectionName = "Auth";

    public required string Issuer { get; init; }
    public required int AccessTokenMinutes { get; init; }
    public required int RefreshTokenDays { get; init; }
    public required bool RequireEmailConfirmed { get; init; }
    public required PasswordConfiguration Password { get; init; }
    public required LockoutConfiguration Lockout { get; init; }
    public ScopeConfiguration Scopes { get; init; } = new();
    public ClientConfiguration[] Clients { get; init; } = [];
    public CertificateConfiguration Certificates { get; init; } = new();

    public sealed record PasswordConfiguration
    {
        public required int RequiredLength { get; init; }
        public required bool RequireDigit { get; init; }
        public required bool RequireLowercase { get; init; }
        public required bool RequireUppercase { get; init; }
        public required bool RequireNonAlphanumeric { get; init; }
    }

    public sealed record LockoutConfiguration
    {
        public required int MaxFailedAccessAttempts { get; init; }
        public required int DefaultLockoutMinutes { get; init; }
    }

    public sealed record ScopeConfiguration
    {
        public bool IncludeProfileScope { get; init; } = true;
    }

    public sealed record ClientConfiguration
    {
        public required string ClientId { get; init; }
        public string? DisplayName { get; init; }
        public string[] RedirectUris { get; init; } = [];
        public string[] PostLogoutRedirectUris { get; init; } = [];
    }

    public sealed record CertificateConfiguration
    {
        public bool UseDevelopmentCertificates { get; init; } = true;
        public CertificateDescriptor? Signing { get; init; }
        public CertificateDescriptor? Encryption { get; init; }
    }

    public sealed record CertificateDescriptor
    {
        public string? Path { get; init; }
        public string? Password { get; init; }
        public string? Thumbprint { get; init; }
        public StoreName StoreName { get; init; } = StoreName.My;
        public StoreLocation StoreLocation { get; init; } = StoreLocation.CurrentUser;
    }
}
