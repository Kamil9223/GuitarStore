using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task Authorize_WhenUserMustChangePassword_ShouldRedirectToForcedChangeScreenInsteadOfIssuingCode()
    {
        const string password = "ChangeMe!123";
        var email = $"auth-step9-authorize-force-{Guid.NewGuid():N}@guitarstore.local";
        var authorizeUrl = BuildAuthorizeUrl("state-step9-force", "nonce-step9-force");

        await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            email,
            password,
            mustChangePassword: true);

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);

        using var authorizeResponse = await client.GetAsync(authorizeUrl);
        authorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        var loginLocation = authorizeResponse.Headers.Location?.ToString();
        loginLocation.ShouldNotBeNull();

        using var loginPageResponse = await client.GetAsync(loginLocation);
        var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(loginPageHtml);

        using var loginResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["EmailOrUserName"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = authorizeUrl
        }));

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        var changePasswordLocation = loginResponse.Headers.Location?.ToString();
        changePasswordLocation.ShouldNotBeNull();
        changePasswordLocation.ShouldContain("/auth/change-password-required");

        using var secondAuthorizeResponse = await client.GetAsync(authorizeUrl);
        secondAuthorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        secondAuthorizeResponse.Headers.Location?.ToString().ShouldContain("/auth/change-password-required");
    }

    [Fact]
    public async Task ForcedPasswordChange_AfterSuccess_ShouldAllowNormalAuthorizeFlow()
    {
        const string currentPassword = "ChangeMe!123";
        const string newPassword = "ChangedAgain!123";
        var email = $"auth-step9-authorize-after-change-{Guid.NewGuid():N}@guitarstore.local";
        var state = $"state-{Guid.NewGuid():N}";
        var authorizeUrl = BuildAuthorizeUrl(state, $"nonce-{Guid.NewGuid():N}");

        await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            email,
            currentPassword,
            mustChangePassword: true);

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);

        using var authorizeResponse = await client.GetAsync(authorizeUrl);
        var loginLocation = authorizeResponse.Headers.Location?.ToString();
        loginLocation.ShouldNotBeNull();

        using var loginPageResponse = await client.GetAsync(loginLocation);
        var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();
        var loginAntiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(loginPageHtml);

        using var loginResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = loginAntiForgeryToken,
            ["EmailOrUserName"] = email,
            ["Password"] = currentPassword,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = authorizeUrl
        }));

        var changePasswordLocation = loginResponse.Headers.Location?.ToString();
        changePasswordLocation.ShouldNotBeNull();
        changePasswordLocation.ShouldContain("/auth/change-password-required");

        using var changePasswordPageResponse = await client.GetAsync(changePasswordLocation);
        var changePasswordHtml = await changePasswordPageResponse.Content.ReadAsStringAsync();
        var changePasswordAntiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(changePasswordHtml);

        using var changePasswordResponse = await client.PostAsync("/auth/change-password-required", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = changePasswordAntiForgeryToken,
            ["CurrentPassword"] = currentPassword,
            ["NewPassword"] = newPassword,
            ["ConfirmPassword"] = newPassword,
            ["ReturnUrl"] = authorizeUrl
        }));

        changePasswordResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        changePasswordResponse.Headers.Location?.ToString().ShouldBe(authorizeUrl);

        using var finalAuthorizeResponse = await client.GetAsync(authorizeUrl);
        finalAuthorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        var callbackLocation = finalAuthorizeResponse.Headers.Location;
        callbackLocation.ShouldNotBeNull();
        callbackLocation.IsAbsoluteUri.ShouldBeTrue();
        callbackLocation.AbsoluteUri.ShouldStartWith(ConfiguredOidcClient.RedirectUri, Case.Sensitive);

        var query = QueryHelpers.ParseQuery(callbackLocation.Query);
        query["code"].ToString().ShouldNotBeNullOrWhiteSpace();
        query["state"].ToString().ShouldBe(state);

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
        user.MustChangePassword.ShouldBeFalse();
        (await userManager.CheckPasswordAsync(user, newPassword)).ShouldBeTrue();
    }

    private static string BuildAuthorizeUrl(string state, string nonce)
    {
        return $"/connect/authorize?client_id={ConfiguredOidcClient.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(ConfiguredOidcClient.RedirectUri)}" +
               "&response_type=code" +
               "&scope=openid%20profile%20offline_access" +
               "&code_challenge=abcdefghijklmnopqrstuvwxyzabcdefghi1234567890AB" +
               "&code_challenge_method=S256" +
               $"&state={Uri.EscapeDataString(state)}" +
               $"&nonce={Uri.EscapeDataString(nonce)}";
    }
}
