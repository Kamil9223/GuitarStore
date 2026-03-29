namespace Tests.EndToEnd.Setup.Modules.Auth;

internal static class ConfiguredOidcClient
{
    internal const string ClientId = "guitarstore-spa";
    internal const string RedirectUri = "http://localhost:3000/auth/callback";
    internal const string PostLogoutRedirectUri = "http://localhost:3000/auth/logout-callback";
}
