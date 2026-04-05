using System.Net;
using Auth.Core.Authorization;
using Shouldly;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AuthorizationProbeTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task CatalogManage_WhenAuthenticatedUserLacksPermission_ShouldRedirectToForbidden()
    {
        var email = $"auth-step5-user-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserWithRolesAsync(
            Scope.ServiceProvider,
            email,
            "ChangeMe!123",
            AuthRoles.User);

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        await LoginAsync(client, email, "ChangeMe!123");

        var response = await client.GetAsync("/authz/probes/catalog-manage");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().ShouldContain("/auth/forbidden");
    }

    [Fact]
    public async Task CatalogManage_WhenAuthenticatedAdmin_ShouldReturnOk()
    {
        var email = $"auth-step5-admin-probe-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserWithRolesAsync(
            Scope.ServiceProvider,
            email,
            "ChangeMe!123",
            AuthRoles.User,
            AuthRoles.Admin);

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        await LoginAsync(client, email, "ChangeMe!123");

        var response = await client.GetAsync("/authz/probes/catalog-manage");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static async Task LoginAsync(HttpClient client, string emailOrUserName, string password, string returnUrl = "/")
    {
        var getResponse = await client.GetAsync($"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["EmailOrUserName"] = emailOrUserName,
            ["Password"] = password,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = returnUrl
        }!));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
    }
}
