using Auth.Core.Authorization;
using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class AuthRolesTest(Setup.Application app) : Setup.EndToEndTestBase(app)
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

    private static async Task<string[]> GetPermissionClaimsAsync(RoleManager<Role> roleManager, Role role)
    {
        return (await roleManager.GetClaimsAsync(role))
            .Where(static claim => claim.Type == AuthClaimTypes.Permission)
            .Select(static claim => claim.Value)
            .OrderBy(static permission => permission, StringComparer.Ordinal)
            .ToArray();
    }
}
