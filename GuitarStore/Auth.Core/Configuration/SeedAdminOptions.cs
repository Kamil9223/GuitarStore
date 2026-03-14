namespace Auth.Core.Configuration;

public sealed record SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public bool Enabled { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
