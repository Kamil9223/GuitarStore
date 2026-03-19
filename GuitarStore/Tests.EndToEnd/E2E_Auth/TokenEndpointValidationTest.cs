using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class TokenEndpointValidationTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task TokenEndpoint_WhenGrantTypeMissing_ShouldReturnOpenIddictError()
    {
        using var client = _webApp.GetHttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent([])
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        responseContent.ShouldContain("\"error\"");
        responseContent.ShouldContain("invalid_request");
    }
}
