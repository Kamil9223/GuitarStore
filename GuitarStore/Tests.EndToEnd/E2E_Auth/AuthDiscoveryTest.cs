using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AuthDiscoveryTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task DiscoveryDocument_ShouldExposeConfiguredMetadata()
    {
        using var client = _webApp.GetHttpsClient();

        var response = await client.GetAsync("/.well-known/openid-configuration");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var discovery = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        discovery.ShouldNotBeNull();

        discovery["issuer"].ToString()!.ShouldBe("https://localhost:7028/");
        discovery["authorization_endpoint"].ToString().ShouldBe("https://localhost:7028/connect/authorize");
        discovery["token_endpoint"].ToString().ShouldBe("https://localhost:7028/connect/token");
        discovery["end_session_endpoint"].ToString().ShouldBe("https://localhost:7028/connect/logout");
        discovery["grant_types_supported"].ToString()!.ShouldContain("authorization_code");
        discovery["grant_types_supported"].ToString()!.ShouldContain("refresh_token");
        discovery["response_types_supported"].ToString()!.ShouldContain("code");
        discovery["scopes_supported"].ToString()!.ShouldContain("openid");
        discovery["scopes_supported"].ToString()!.ShouldContain("offline_access");
    }
}


