using Duende.IdentityServer.Models;

namespace Auth.Core;
public static class DuendeConfig
{
    // Standardowe zasoby tożsamości OIDC
    public static IReadOnlyCollection<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    ];

    // Scopes API – granularne permisje
    public static IReadOnlyCollection<ApiScope> ApiScopes =>
    [
        new ApiScope("guitarstore.orders.read"),
        new ApiScope("guitarstore.orders.write"),
    ];

    // API resource (audience) – grupuje scopes; można dodać claims
    public static IReadOnlyCollection<ApiResource> ApiResources =>
    [
        new ApiResource("guitarstore-api", "GuitarStore API")
        {
            Scopes = { "guitarstore.orders.read", "guitarstore.orders.write" },
            UserClaims = { "role", "permissions", "customer_id" } // jeśli chcesz mieć te claimy w access token
        }
    ];

    // Klienci
    public static IReadOnlyCollection<Client> Clients =>
    [
        // 1) Client Credentials – integracje M2M, testy Postman/curl
        new Client
        {
            ClientId = "gs.svc.cli",
            ClientName = "GuitarStore Service Client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { "guitarstore.orders.read", "guitarstore.orders.write" }
        },

        // 2) Authorization Code + PKCE – Postman / SPA w DEV
        new Client
        {
            ClientId = "gs.postman",
            ClientName = "GuitarStore Postman",
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false, // jak SPA / Postman
            RedirectUris = { "https://oauth.pstmn.io/v1/callback" }, // Postman callback
            AllowedScopes = { "openid", "profile", "guitarstore.orders.read", "guitarstore.orders.write" },
            AllowedCorsOrigins = { "https://oauth.pstmn.io" },
            AccessTokenLifetime = 900 // 15 min
        }
    ];
}
