using Auth.Core.Authorization;
using Auth.Core.Entities;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Auth.Core.Configuration;

internal sealed class AuthRolesInitializer(
    RoleManager<Role> roleManager,
    ILogger<AuthRolesInitializer> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in AuthRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var role = await EnsureRoleAsync(roleName);
            await SyncPermissionClaimsAsync(role, AuthRolePermissions.GetPermissions(roleName));
        }
    }

    private async Task<Role> EnsureRoleAsync(string roleName)
    {
        var existingRole = await roleManager.FindByNameAsync(roleName);
        if (existingRole is not null)
        {
            return existingRole;
        }

        var role = new Role
        {
            Id = AuthId.New(),
            Name = roleName
        };

        EnsureSuccess(await roleManager.CreateAsync(role), $"creating role '{roleName}'");
        logger.LogInformation("Registered auth role '{RoleName}'.", roleName);

        return await roleManager.FindByNameAsync(roleName)
            ?? throw new InvalidOperationException($"Auth role '{roleName}' should exist after creation.");
    }

    private async Task SyncPermissionClaimsAsync(Role role, IReadOnlyCollection<string> expectedPermissions)
    {
        var existingPermissionClaims = (await roleManager.GetClaimsAsync(role))
            .Where(static claim => claim.Type == AuthClaimTypes.Permission)
            .Select(static claim => claim.Value)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var permission in expectedPermissions.Except(existingPermissionClaims, StringComparer.Ordinal))
        {
            EnsureSuccess(
                await roleManager.AddClaimAsync(role, new Claim(AuthClaimTypes.Permission, permission)),
                $"adding permission '{permission}' to role '{role.Name}'");

            logger.LogInformation("Granted permission '{Permission}' to role '{RoleName}'.", permission, role.Name);
        }

        foreach (var permission in existingPermissionClaims.Except(expectedPermissions, StringComparer.Ordinal))
        {
            EnsureSuccess(
                await roleManager.RemoveClaimAsync(role, new Claim(AuthClaimTypes.Permission, permission)),
                $"removing permission '{permission}' from role '{role.Name}'");

            logger.LogInformation("Removed permission '{Permission}' from role '{RoleName}'.", permission, role.Name);
        }
    }

    private static void EnsureSuccess(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(static error => error.Description));
        throw new InvalidOperationException($"Auth roles initialization failed while {operation}: {errors}");
    }
}
