using Shouldly;
using System.Net;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AuthorizationCodeFlowTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task AuthorizeAndTokenExchange_ShouldReturnTokens()
    {
        var email = $"auth-step8-code-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserAsync(Scope.ServiceProvider, email, "ChangeMe!123");

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var authorizationResult = await OidcAuthorizationCodeFlowTestHelper.AuthorizeWithLoginAsync(client, email, "ChangeMe!123");

        var tokenResult = await OidcAuthorizationCodeFlowTestHelper.ExchangeCodeAsync(client, authorizationResult);

        tokenResult.StatusCode.ShouldBe(HttpStatusCode.OK, tokenResult.RawContent);
        tokenResult.Payload.TokenType.ShouldBe("Bearer");
        tokenResult.Payload.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokenResult.Payload.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        var grantedScope = tokenResult.Payload.Scope;
        grantedScope.ShouldNotBeNullOrWhiteSpace();
        grantedScope.ShouldContain("openid");
        grantedScope.ShouldContain("profile");
        grantedScope.ShouldContain("offline_access");
        tokenResult.Payload.ExpiresIn.ShouldNotBeNull();
        tokenResult.Payload.ExpiresIn.Value.ShouldBeGreaterThan(0);
        authorizationResult.CallbackUri.ToString().ShouldContain($"state={authorizationResult.State}");
    }

    [Fact]
    public async Task RefreshToken_ShouldRotateRefreshToken()
    {
        var email = $"auth-step8-refresh-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserAsync(Scope.ServiceProvider, email, "ChangeMe!123");

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var authorizationResult = await OidcAuthorizationCodeFlowTestHelper.AuthorizeWithLoginAsync(client, email, "ChangeMe!123");
        var tokenResult = await OidcAuthorizationCodeFlowTestHelper.ExchangeCodeAsync(client, authorizationResult);
        var originalRefreshToken = tokenResult.Payload.RefreshToken;

        originalRefreshToken.ShouldNotBeNullOrWhiteSpace();

        var refreshResult = await OidcAuthorizationCodeFlowTestHelper.RefreshAsync(client, originalRefreshToken);

        refreshResult.StatusCode.ShouldBe(HttpStatusCode.OK, refreshResult.RawContent);
        refreshResult.Payload.AccessToken.ShouldNotBeNullOrWhiteSpace();
        refreshResult.Payload.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        refreshResult.Payload.RefreshToken.ShouldNotBe(originalRefreshToken);
    }

    [Fact]
    public async Task RefreshTokenReuse_ShouldBeRejected()
    {
        var email = $"auth-step8-refresh-reuse-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserAsync(Scope.ServiceProvider, email, "ChangeMe!123");

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var authorizationResult = await OidcAuthorizationCodeFlowTestHelper.AuthorizeWithLoginAsync(client, email, "ChangeMe!123");
        var tokenResult = await OidcAuthorizationCodeFlowTestHelper.ExchangeCodeAsync(client, authorizationResult);
        var originalRefreshToken = tokenResult.Payload.RefreshToken;

        originalRefreshToken.ShouldNotBeNullOrWhiteSpace();

        var firstRefreshResult = await OidcAuthorizationCodeFlowTestHelper.RefreshAsync(client, originalRefreshToken);
        firstRefreshResult.StatusCode.ShouldBe(HttpStatusCode.OK, firstRefreshResult.RawContent);

        var secondRefreshResult = await OidcAuthorizationCodeFlowTestHelper.RefreshAsync(client, originalRefreshToken);

        secondRefreshResult.StatusCode.ShouldBe(HttpStatusCode.BadRequest, secondRefreshResult.RawContent);
        secondRefreshResult.Payload.Error.ShouldBe("invalid_grant");
        secondRefreshResult.Payload.ErrorDescription.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AuthorizationCode_WithInvalidVerifier_ShouldBeRejected()
    {
        var email = $"auth-step8-bad-verifier-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserAsync(Scope.ServiceProvider, email, "ChangeMe!123");

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var authorizationResult = await OidcAuthorizationCodeFlowTestHelper.AuthorizeWithLoginAsync(client, email, "ChangeMe!123");

        var tokenResult = await OidcAuthorizationCodeFlowTestHelper.ExchangeCodeAsync(
            client,
            authorizationResult,
            codeVerifierOverride: "invalid-code-verifier");

        tokenResult.StatusCode.ShouldBe(HttpStatusCode.BadRequest, tokenResult.RawContent);
        tokenResult.Payload.Error.ShouldBe("invalid_grant");
        tokenResult.Payload.AccessToken.ShouldBeNull();
    }

    [Fact]
    public async Task AuthorizationCode_ReusedSecondTime_ShouldBeRejected()
    {
        var email = $"auth-step8-code-reuse-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserAsync(Scope.ServiceProvider, email, "ChangeMe!123");

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var authorizationResult = await OidcAuthorizationCodeFlowTestHelper.AuthorizeWithLoginAsync(client, email, "ChangeMe!123");

        var firstTokenResult = await OidcAuthorizationCodeFlowTestHelper.ExchangeCodeAsync(client, authorizationResult);
        firstTokenResult.StatusCode.ShouldBe(HttpStatusCode.OK, firstTokenResult.RawContent);

        var secondTokenResult = await OidcAuthorizationCodeFlowTestHelper.ExchangeCodeAsync(client, authorizationResult);

        secondTokenResult.StatusCode.ShouldBe(HttpStatusCode.BadRequest, secondTokenResult.RawContent);
        secondTokenResult.Payload.Error.ShouldBe("invalid_grant");
        secondTokenResult.Payload.AccessToken.ShouldBeNull();
    }
}
