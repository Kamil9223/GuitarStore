using Microsoft.Extensions.Configuration.Memory;

namespace Tests.EndToEnd.Setup;
internal static class MemoryConfigurationTestSource
{
    internal static MemoryConfigurationSource BuildConfiguration(TestsContainers containers)
    {
        return new MemoryConfigurationSource
        {
            InitialData =
            [
                new("ConnectionStrings:GuitarStore", containers.MsSqlContainerConnectionString),
                new("ConnectionStrings:RabbitMq", containers.RabbitMqContainerConnectionString),
                new("Stripe:Url", containers.StripeBaseUrl),
                new("Stripe:SecretKey", "sk_test_123"),
                new("Auth:Issuer", "https://localhost:7028"),
                new("Auth:AccessTokenMinutes", "15"),
                new("Auth:RefreshTokenDays", "30"),
                new("Auth:RequireEmailConfirmed", "false"),
                new("Auth:Scopes:IncludeProfileScope", "true"),
                new("Auth:Clients:0:ClientId", "guitarstore-spa"),
                new("Auth:Clients:0:DisplayName", "GuitarStore SPA"),
                new("Auth:Clients:0:RedirectUris:0", "http://localhost:3000/auth/callback"),
                new("Auth:Clients:0:PostLogoutRedirectUris:0", "http://localhost:3000/auth/logout-callback"),
                new("Auth:Certificates:UseDevelopmentCertificates", "true"),
                new("Auth:Password:RequiredLength", "8"),
                new("Auth:Password:RequireDigit", "true"),
                new("Auth:Password:RequireLowercase", "true"),
                new("Auth:Password:RequireUppercase", "true"),
                new("Auth:Password:RequireNonAlphanumeric", "true"),
                new("Auth:Lockout:MaxFailedAccessAttempts", "5"),
                new("Auth:Lockout:DefaultLockoutMinutes", "10"),
                new("SeedAdmin:Enabled", "false")
            ]
        };
    }
}
