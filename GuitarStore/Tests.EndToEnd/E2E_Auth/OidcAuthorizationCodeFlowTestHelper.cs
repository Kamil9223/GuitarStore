using Microsoft.AspNetCore.WebUtilities;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tests.EndToEnd.Setup.Modules.Auth;

namespace Tests.EndToEnd.E2E_Auth;

internal static class OidcAuthorizationCodeFlowTestHelper
{
    internal static async Task<OidcAuthorizationCodeResult> AuthorizeWithLoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        var pkce = PkcePair.Create();
        var state = $"state-{Guid.NewGuid():N}";
        var nonce = $"nonce-{Guid.NewGuid():N}";
        var authorizeUrl = BuildAuthorizeUrl(pkce.CodeChallenge, state, nonce);

        using var authorizeResponse = await client.GetAsync(authorizeUrl);
        authorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        var loginLocation = authorizeResponse.Headers.Location?.ToString();
        loginLocation.ShouldNotBeNull();
        loginLocation.ShouldContain("/auth/login");

        using var loginPageResponse = await client.GetAsync(loginLocation);
        loginPageResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
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

        var returnToAuthorizeLocation = loginResponse.Headers.Location?.ToString();
        returnToAuthorizeLocation.ShouldBe(authorizeUrl);

        using var authorizedResponse = await client.GetAsync(returnToAuthorizeLocation);
        authorizedResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        var callbackLocation = authorizedResponse.Headers.Location;
        callbackLocation.ShouldNotBeNull();
        callbackLocation.IsAbsoluteUri.ShouldBeTrue();
        callbackLocation.AbsoluteUri.ShouldStartWith(ConfiguredOidcClient.RedirectUri, Case.Sensitive);

        var query = QueryHelpers.ParseQuery(callbackLocation.Query);
        var authorizationCode = query["code"].ToString();
        var returnedState = query["state"].ToString();

        authorizationCode.ShouldNotBeNullOrWhiteSpace();
        returnedState.ShouldBe(state);

        return new OidcAuthorizationCodeResult(
            authorizationCode,
            pkce.CodeVerifier,
            state,
            callbackLocation,
            authorizeUrl);
    }

    internal static Task<OidcTokenEndpointResult> ExchangeCodeAsync(
        HttpClient client,
        OidcAuthorizationCodeResult authorizationCodeResult,
        string? codeVerifierOverride = null)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = ConfiguredOidcClient.ClientId,
            ["code"] = authorizationCodeResult.Code,
            ["redirect_uri"] = ConfiguredOidcClient.RedirectUri,
            ["code_verifier"] = codeVerifierOverride ?? authorizationCodeResult.CodeVerifier
        };

        return SendTokenRequestAsync(client, form);
    }

    internal static Task<OidcTokenEndpointResult> RefreshAsync(HttpClient client, string refreshToken)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = ConfiguredOidcClient.ClientId,
            ["refresh_token"] = refreshToken
        };

        return SendTokenRequestAsync(client, form);
    }

    private static string BuildAuthorizeUrl(string codeChallenge, string state, string nonce)
    {
        return $"/connect/authorize?client_id={ConfiguredOidcClient.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(ConfiguredOidcClient.RedirectUri)}" +
               "&response_type=code" +
               "&scope=openid%20profile%20offline_access" +
               $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
               "&code_challenge_method=S256" +
               $"&state={Uri.EscapeDataString(state)}" +
               $"&nonce={Uri.EscapeDataString(nonce)}";
    }

    private static async Task<OidcTokenEndpointResult> SendTokenRequestAsync(HttpClient client, IDictionary<string, string> form)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(form)
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request);
        var rawContent = await response.Content.ReadAsStringAsync();
        var payload = string.IsNullOrWhiteSpace(rawContent)
            ? new OidcTokenResponse()
            : JsonSerializer.Deserialize<OidcTokenResponse>(rawContent) ?? new OidcTokenResponse();

        return new OidcTokenEndpointResult(response.StatusCode, payload, rawContent);
    }

    internal sealed record OidcAuthorizationCodeResult(
        string Code,
        string CodeVerifier,
        string State,
        Uri CallbackUri,
        string AuthorizeUrl);

    internal sealed record OidcTokenEndpointResult(
        HttpStatusCode StatusCode,
        OidcTokenResponse Payload,
        string RawContent);

    private sealed record PkcePair(string CodeVerifier, string CodeChallenge)
    {
        internal static PkcePair Create()
        {
            Span<byte> buffer = stackalloc byte[32];
            RandomNumberGenerator.Fill(buffer);

            var codeVerifier = Base64UrlEncode(buffer);
            var codeChallenge = CreateCodeChallenge(codeVerifier);

            return new PkcePair(codeVerifier, codeChallenge);
        }

        private static string CreateCodeChallenge(string codeVerifier)
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
