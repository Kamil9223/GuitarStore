using System.Security.Claims;
using Auth.Core.Authorization;
using GuitarStore.ApiGateway.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Shouldly;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class OidcPrincipalFactoryTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task WhenUserHasAdminRole_ShouldIncludeRoleAndPermissionClaims()
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
}
