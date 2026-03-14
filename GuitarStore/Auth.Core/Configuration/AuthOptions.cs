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
}
