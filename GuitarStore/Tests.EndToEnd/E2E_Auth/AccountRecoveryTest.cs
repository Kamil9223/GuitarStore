using Auth.Core.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Net;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AccountRecoveryTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task Register_WhenEmailConfirmationRequired_ShouldNotSignInAndShouldSendConfirmationEmail()
    {
        using var configuredApp = CreateAppWithEmailConfirmationRequired();
        using var scope = configuredApp.Services.CreateScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<TestAuthEmailSender>();
        var cookieOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var identityCookieName = cookieOptions.Get(IdentityConstants.ApplicationScheme).Cookie.Name;
        var email = $"auth-step10-register-confirm-{Guid.NewGuid():N}@guitarstore.local";

        using var client = CreateHttpsClient(configuredApp, allowAutoRedirect: false);
        var getResponse = await client.GetAsync("/auth/register");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Name"] = "Confirm",
            ["LastName"] = "Required",
            ["Email"] = email,
            ["Password"] = "ChangeMe!123",
            ["ConfirmPassword"] = "ChangeMe!123",
            ["ReturnUrl"] = "/"
        }));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResponse.Headers.Location?.ToString().ShouldStartWith("/auth/register-confirmation");
        var setCookies = postResponse.Headers.TryGetValues("Set-Cookie", out var headerValues)
            ? headerValues
            : [];
        setCookies!.Any(cookie => cookie.Contains(identityCookieName!, StringComparison.Ordinal)).ShouldBeFalse();

        var confirmationEmail = emailSender.FindLatestEmailConfirmation(email);
        confirmationEmail.ShouldNotBeNull();
        confirmationEmail.ConfirmationLink.AbsolutePath.ShouldBe("/auth/confirm-email");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
        user.EmailConfirmed.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ShouldMarkUserAsConfirmed()
    {
        using var configuredApp = CreateAppWithEmailConfirmationRequired();
        using var scope = configuredApp.Services.CreateScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<TestAuthEmailSender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var email = $"auth-step10-confirm-email-{Guid.NewGuid():N}@guitarstore.local";

        using var client = CreateHttpsClient(configuredApp, allowAutoRedirect: false);
        await RegisterUserAsync(client, email);

        var confirmationEmail = emailSender.FindLatestEmailConfirmation(email);
        confirmationEmail.ShouldNotBeNull();

        using var confirmResponse = await client.GetAsync(confirmationEmail.ConfirmationLink);
        var confirmHtml = await confirmResponse.Content.ReadAsStringAsync();

        confirmResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        confirmHtml.ShouldContain("Email confirmed");

        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
        user.EmailConfirmed.ShouldBeTrue();
    }

    [Fact]
    public async Task Login_WhenEmailIsNotConfirmed_ShouldShowConfirmationMessage()
    {
        using var configuredApp = CreateAppWithEmailConfirmationRequired();
        using var scope = configuredApp.Services.CreateScope();
        var email = $"auth-step10-login-unconfirmed-{Guid.NewGuid():N}@guitarstore.local";

        await AuthTestDataSeeder.EnsureUserAsync(
            scope.ServiceProvider,
            email,
            "ChangeMe!123",
            emailConfirmed: false);

        using var client = CreateHttpsClient(configuredApp, allowAutoRedirect: false);
        var getResponse = await client.GetAsync("/auth/login");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["EmailOrUserName"] = email,
            ["Password"] = "ChangeMe!123",
            ["RememberMe"] = "false",
            ["ReturnUrl"] = "/"
        }));

        var postHtml = await postResponse.Content.ReadAsStringAsync();

        postResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        postHtml.ShouldContain("Please confirm your email before signing in.");
    }

    [Fact]
    public async Task ForgotPassword_ForExistingConfirmedUser_ShouldSendResetEmail()
    {
        var email = $"auth-step10-forgot-{Guid.NewGuid():N}@guitarstore.local";
        await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            email,
            "ChangeMe!123",
            emailConfirmed: true);

        var emailSender = Scope.ServiceProvider.GetRequiredService<TestAuthEmailSender>();

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var getResponse = await client.GetAsync("/auth/forgot-password");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/forgot-password", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Email"] = email
        }));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResponse.Headers.Location?.ToString().ShouldBe("/auth/forgot-password-confirmation");

        var resetEmail = emailSender.FindLatestPasswordReset(email);
        resetEmail.ShouldNotBeNull();
        resetEmail.ResetLink.AbsolutePath.ShouldBe("/auth/reset-password");
    }

    [Fact]
    public async Task ForgotPassword_ForUnknownEmail_ShouldReturnSameConfirmationWithoutSendingSensitiveSignal()
    {
        var email = $"auth-step10-unknown-{Guid.NewGuid():N}@guitarstore.local";
        var emailSender = Scope.ServiceProvider.GetRequiredService<TestAuthEmailSender>();

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        var getResponse = await client.GetAsync("/auth/forgot-password");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/forgot-password", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Email"] = email
        }));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResponse.Headers.Location?.ToString().ShouldBe("/auth/forgot-password-confirmation");
        emailSender.FindLatestPasswordReset(email).ShouldBeNull();
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldAllowLoginWithNewPassword()
    {
        const string oldPassword = "ChangeMe!123";
        const string newPassword = "ResetWorks!123";
        var email = $"auth-step10-reset-valid-{Guid.NewGuid():N}@guitarstore.local";

        await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            email,
            oldPassword,
            emailConfirmed: true);

        var emailSender = Scope.ServiceProvider.GetRequiredService<TestAuthEmailSender>();

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        await RequestPasswordResetAsync(client, email);

        var resetEmail = emailSender.FindLatestPasswordReset(email);
        resetEmail.ShouldNotBeNull();

        using var resetPageResponse = await client.GetAsync(resetEmail.ResetLink);
        var resetPageHtml = await resetPageResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(resetPageHtml);
        var query = QueryHelpers.ParseQuery(resetEmail.ResetLink.Query);

        using var postResetResponse = await client.PostAsync("/auth/reset-password", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["UserId"] = query["userId"].ToString(),
            ["Token"] = query["token"].ToString(),
            ["NewPassword"] = newPassword,
            ["ConfirmPassword"] = newPassword
        }));

        postResetResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        postResetResponse.Headers.Location?.ToString().ShouldBe("/auth/reset-password-confirmation");

        var loginPageResponse = await client.GetAsync("/auth/login?returnUrl=%2F");
        var loginHtml = await loginPageResponse.Content.ReadAsStringAsync();
        var loginAntiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(loginHtml);

        using var loginResponse = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = loginAntiForgeryToken,
            ["EmailOrUserName"] = email,
            ["Password"] = newPassword,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = "/"
        }));

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
        loginResponse.Headers.Location?.ToString().ShouldBe("/");
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ShouldShowValidationError()
    {
        var email = $"auth-step10-reset-invalid-{Guid.NewGuid():N}@guitarstore.local";

        var user = await AuthTestDataSeeder.EnsureUserAsync(
            Scope.ServiceProvider,
            email,
            "ChangeMe!123",
            emailConfirmed: true);

        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);
        using var resetPageResponse = await client.GetAsync($"/auth/reset-password?userId={Uri.EscapeDataString(user.Id.ToString())}&token=bad-token");
        var resetPageHtml = await resetPageResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(resetPageHtml);

        using var postResetResponse = await client.PostAsync("/auth/reset-password", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["UserId"] = user.Id.ToString(),
            ["Token"] = "bad-token",
            ["NewPassword"] = "AnotherGood!123",
            ["ConfirmPassword"] = "AnotherGood!123"
        }));

        var postResetHtml = await postResetResponse.Content.ReadAsStringAsync();

        postResetResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        postResetHtml.ShouldContain("This password reset link is invalid or has expired.");
    }

    private static HttpClient CreateHttpsClient(WebApplicationFactory<Program> app, bool allowAutoRedirect)
    {
        return app.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:7028"),
            AllowAutoRedirect = allowAutoRedirect
        });
    }

    private WebApplicationFactory<Program> CreateAppWithEmailConfirmationRequired()
    {
        return _webApp.Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Auth:RequireEmailConfirmed"] = "true"
                });
            });
        });
    }

    private static async Task RegisterUserAsync(HttpClient client, string email)
    {
        var getResponse = await client.GetAsync("/auth/register");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Name"] = "Email",
            ["LastName"] = "Confirmation",
            ["Email"] = email,
            ["Password"] = "ChangeMe!123",
            ["ConfirmPassword"] = "ChangeMe!123",
            ["ReturnUrl"] = "/"
        }));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
    }

    private static async Task RequestPasswordResetAsync(HttpClient client, string email)
    {
        var getResponse = await client.GetAsync("/auth/forgot-password");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = AuthUiTestHelpers.ExtractAntiForgeryToken(html);

        using var postResponse = await client.PostAsync("/auth/forgot-password", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Email"] = email
        }));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);
    }
}
