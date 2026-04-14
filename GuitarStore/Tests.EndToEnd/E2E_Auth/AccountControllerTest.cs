using Auth.Core.Authorization;
using Auth.Core.Entities;
using Auth.Core.Events.Outgoing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using Tests.EndToEnd.Setup.Modules.Auth;
using Tests.EndToEnd.Setup.TestsHelpers;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AccountControllerTest(Setup.Application app) : Setup.EndToEndTestBase(app)
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
    public async Task Register_WhenPostedWithValidData_ShouldCreateUserPublishIntegrationEventAndRedirectHome()
    {
        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var publishedEvent = RabbitMqChannel.CreateTestConsumerForPublishing<UserRegisteredEvent>();

        var getResponse = await client.GetAsync("/auth/register");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);
        var email = $"auth-step3-{Guid.NewGuid():N}@guitarstore.local";

        using var postResponse = await client.PostAsync("/auth/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Name"] = "Test",
            ["LastName"] = "User",
            ["Email"] = email,
            ["Password"] = "ChangeMe!123",
            ["ConfirmPassword"] = "ChangeMe!123",
            ["ReturnUrl"] = "/"
        }!));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResponse.Headers.Location?.ToString().ShouldBe("/");
        postResponse.Headers.Contains("Set-Cookie").ShouldBeTrue();

        await publishedEvent.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
    }

    [Fact]
    public async Task Register_ShouldAssignDefaultUserRole()
    {
        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);

        var getResponse = await client.GetAsync("/auth/register");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);
        var email = $"auth-step5-register-{Guid.NewGuid():N}@guitarstore.local";

        using var postResponse = await client.PostAsync("/auth/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Name"] = "Step5",
            ["LastName"] = "User",
            ["Email"] = email,
            ["Password"] = "ChangeMe!123",
            ["ConfirmPassword"] = "ChangeMe!123",
            ["ReturnUrl"] = "/"
        }));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
        (await userManager.IsInRoleAsync(user, AuthRoles.User)).ShouldBeTrue();
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

    [Fact]
    public async Task Login_WhenUserMustChangePassword_ShouldRedirectToForcedChangeScreen()
    {
        const string password = "ChangeMe!123";
        const string returnUrl = "/orders/history";
        var email = $"auth-step9-login-force-{Guid.NewGuid():N}@guitarstore.local";

        await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            email,
            password,
            mustChangePassword: true);

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var getResponse = await client.GetAsync($"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["EmailOrUserName"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = returnUrl
        }));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResponse.Headers.Location?.ToString().ShouldContain("/auth/change-password-required");
        postResponse.Headers.Location?.ToString().ShouldContain(Uri.EscapeDataString(returnUrl));
        postResponse.Headers.Contains("Set-Cookie").ShouldBeTrue();
    }

    [Fact]
    public async Task ForcedPasswordChange_WhenPostedWithValidData_ShouldClearFlagAndRedirectToReturnUrl()
    {
        const string currentPassword = "ChangeMe!123";
        const string newPassword = "EvenBetter!123";
        const string returnUrl = "/orders/history";
        var email = $"auth-step9-change-password-{Guid.NewGuid():N}@guitarstore.local";

        await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            email,
            currentPassword,
            mustChangePassword: true);

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var loginPageResponse = await client.GetAsync($"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();
        var loginAntiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(loginPageHtml);

        using var loginResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = loginAntiForgeryToken,
            ["EmailOrUserName"] = email,
            ["Password"] = currentPassword,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = returnUrl
        }));

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        var changePasswordLocation = loginResponse.Headers.Location?.ToString();
        changePasswordLocation.ShouldNotBeNull();

        var changePasswordPageResponse = await client.GetAsync(changePasswordLocation);
        var changePasswordHtml = await changePasswordPageResponse.Content.ReadAsStringAsync();
        var changePasswordAntiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(changePasswordHtml);

        using var changePasswordResponse = await client.PostAsync("/auth/change-password-required", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = changePasswordAntiForgeryToken,
            ["CurrentPassword"] = currentPassword,
            ["NewPassword"] = newPassword,
            ["ConfirmPassword"] = newPassword,
            ["ReturnUrl"] = returnUrl
        }));

        changePasswordResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        changePasswordResponse.Headers.Location?.ToString().ShouldBe(returnUrl);

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
        user.MustChangePassword.ShouldBeFalse();
        (await userManager.CheckPasswordAsync(user, newPassword)).ShouldBeTrue();
    }
}
