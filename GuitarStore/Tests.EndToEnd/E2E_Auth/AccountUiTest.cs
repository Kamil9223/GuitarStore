using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AccountUiTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task LoginPage_ShouldRenderForm()
    {
        using var client = _webApp.GetHttpsClient();

        var response = await client.GetAsync("/auth/login");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        html.ShouldContain("<form");
        html.ShouldContain("/auth/login");
        html.ShouldContain("__RequestVerificationToken");
    }

    [Fact]
    public async Task Register_WhenPostedWithValidData_ShouldCreateUserAndRedirectHome()
    {
        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);

        var getResponse = await client.GetAsync("/auth/register");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);
        var email = $"auth-step3-{Guid.NewGuid():N}@guitarstore.local";

        using var postResponse = await client.PostAsync("/auth/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Email"] = email,
            ["Password"] = "ChangeMe!123",
            ["ConfirmPassword"] = "ChangeMe!123",
            ["ReturnUrl"] = "/"
        }!));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResponse.Headers.Location?.ToString().ShouldBe("/");
        postResponse.Headers.Contains("Set-Cookie").ShouldBeTrue();

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldRedirectToProvidedReturnUrl()
    {
        await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            "account-ui-login@guitarstore.local",
            "ChangeMe!123");

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var returnUrl = $"/connect/authorize?client_id={ConfiguredOidcClient.ClientId}&redirect_uri={Uri.EscapeDataString(ConfiguredOidcClient.RedirectUri)}&response_type=code&scope=openid%20profile%20offline_access&code_challenge=abcdefghijklmnopqrstuvwxyzabcdefghi1234567890AB&code_challenge_method=S256&state=test-state&nonce=test-nonce";

        var getResponse = await client.GetAsync($"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["EmailOrUserName"] = "account-ui-login@guitarstore.local",
            ["Password"] = "ChangeMe!123",
            ["RememberMe"] = "false",
            ["ReturnUrl"] = returnUrl
        }!));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResponse.Headers.Location?.ToString().ShouldBe(returnUrl);
        postResponse.Headers.Contains("Set-Cookie").ShouldBeTrue();
    }
}
