using Shouldly;
using System.Net;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AuthorizeEndpointTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task Authorize_WhenUserIsNotAuthenticated_ShouldRedirectToLoginUi()
    {
        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);

        var response = await client.GetAsync($"/connect/authorize?client_id={ConfiguredOidcClient.ClientId}&redirect_uri={Uri.EscapeDataString(ConfiguredOidcClient.RedirectUri)}&response_type=code&scope=openid%20profile%20offline_access&code_challenge=abcdefghijklmnopqrstuvwxyzabcdefghi1234567890AB&code_challenge_method=S256&state=test-state&nonce=test-nonce");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        var location = response.Headers.Location?.ToString();
        location.ShouldNotBeNull();
        location.ShouldContain("/auth/login");
        location.ShouldContain("ReturnUrl=");
        location.ShouldContain("%2Fconnect%2Fauthorize");
    }
}
