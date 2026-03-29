using Auth.Core.Authorization;
using Auth.Core.Entities;
using GuitarStore.ApiGateway.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Shouldly;
using System.Net;
using System.Security.Claims;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AuthStep5Tests(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task DefaultRoles_ShouldBeSeededWithExpectedPermissionClaims()
    {
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        var userRole = await roleManager.FindByNameAsync(AuthRoles.User);
        var supportRole = await roleManager.FindByNameAsync(AuthRoles.Support);
        var adminRole = await roleManager.FindByNameAsync(AuthRoles.Admin);

        userRole.ShouldNotBeNull();
        supportRole.ShouldNotBeNull();
        adminRole.ShouldNotBeNull();

        var userPermissions = await GetPermissionClaimsAsync(roleManager, userRole);
        var supportPermissions = await GetPermissionClaimsAsync(roleManager, supportRole);
        var adminPermissions = await GetPermissionClaimsAsync(roleManager, adminRole);

        userPermissions.ShouldBeEmpty();
        supportPermissions.ShouldBe(
        [
            AuthPermissions.CustomersViewAny,
            AuthPermissions.OrdersCancelAny,
            AuthPermissions.OrdersViewAny
        ]);
        adminPermissions.ShouldBe(
        [
            AuthPermissions.CatalogManage,
            AuthPermissions.CustomersViewAny,
            AuthPermissions.OrdersCancelAny,
            AuthPermissions.OrdersViewAny
        ]);
    }

    [Fact]
    public async Task Register_ShouldAssignDefaultUserRole()
    {
        using var client = _webApp.GetHttpsClient(allowAutoRedirect: false);

        var getResponse = await client.GetAsync("/auth/register");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = ExtractAntiForgeryToken(html);
        var email = $"auth-step5-register-{Guid.NewGuid():N}@guitarstore.local";

        using var postResponse = await client.PostAsync("/auth/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = antiForgeryToken,
            ["Email"] = email,
            ["Password"] = "ChangeMe!123",
            ["ConfirmPassword"] = "ChangeMe!123",
            ["ReturnUrl"] = "/"
        }!));

        postResponse.StatusCode.ShouldBe(HttpStatusCode.Found);

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        user.ShouldNotBeNull();
        (await userManager.IsInRoleAsync(user, AuthRoles.User)).ShouldBeTrue();
    }

    [Fact]
    public async Task OidcPrincipalFactory_WhenUserHasAdminRole_ShouldIncludeRoleAndPermissionClaims()
    {
        var user = await AuthTestDataSeeder.EnsureUserWithRolesAsync(
            Scope.ServiceProvider,
            $"auth-step5-admin-{Guid.NewGuid():N}@guitarstore.local",
            "ChangeMe!123",
            AuthRoles.User,
            AuthRoles.Admin);

        var principalFactory = Scope.ServiceProvider.GetRequiredService<IOidcClaimsPrincipalFactory>();
        var authorizationService = Scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

        var principal = await principalFactory.CreateAsync(user, new OpenIddictRequest
        {
            Scope = "openid profile offline_access"
        });

        principal.Claims.ShouldContain(static claim => claim.Type == ClaimTypes.Role && claim.Value == AuthRoles.Admin);
        principal.Claims.ShouldContain(static claim => claim.Type == AuthClaimTypes.Permission && claim.Value == AuthPermissions.CatalogManage);
        principal.Claims.ShouldContain(static claim => claim.Type == AuthClaimTypes.Permission && claim.Value == AuthPermissions.OrdersViewAny);

        var authorizationResult = await authorizationService.AuthorizeAsync(principal, resource: null, AuthPolicies.CatalogManage);
        authorizationResult.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task CatalogManageProbe_WhenAuthenticatedUserLacksPermission_ShouldRedirectToForbidden()
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
    public async Task CatalogManageProbe_WhenAuthenticatedAdmin_ShouldReturnOk()
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

    private static async Task<string[]> GetPermissionClaimsAsync(RoleManager<Role> roleManager, Role role)
    {
        return (await roleManager.GetClaimsAsync(role))
            .Where(static claim => claim.Type == AuthClaimTypes.Permission)
            .Select(static claim => claim.Value)
            .OrderBy(static permission => permission, StringComparer.Ordinal)
            .ToArray();
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        const string pattern = "name=\"__RequestVerificationToken\"";
        var nameIndex = html.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
        if (nameIndex < 0)
        {
            throw new InvalidOperationException("Anti-forgery token was not found in the HTML response.");
        }

        var valuePattern = "value=\"";
        var valueStart = html.IndexOf(valuePattern, nameIndex, StringComparison.OrdinalIgnoreCase);
        if (valueStart < 0)
        {
            throw new InvalidOperationException("Anti-forgery token value was not found in the HTML response.");
        }

        valueStart += valuePattern.Length;
        var valueEnd = html.IndexOf('"', valueStart);
        if (valueEnd < 0)
        {
            throw new InvalidOperationException("Anti-forgery token value was not terminated in the HTML response.");
        }

        return html[valueStart..valueEnd];
    }

    private static async Task LoginAsync(HttpClient client, string emailOrUserName, string password, string returnUrl = "/")
    {
        var getResponse = await client.GetAsync($"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        var html = await getResponse.Content.ReadAsStringAsync();
        var antiForgeryToken = ExtractAntiForgeryToken(html);

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
