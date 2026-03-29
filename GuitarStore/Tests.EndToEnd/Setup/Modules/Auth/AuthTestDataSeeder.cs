using Auth.Core.Authorization;
using Auth.Core.Entities;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.EndToEnd.Setup.Modules.Auth;

internal static class AuthTestDataSeeder
{
    internal static async Task<User> EnsureUserAsync(IServiceProvider serviceProvider, string email, string password)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            if (await userManager.HasPasswordAsync(existingUser) && !await userManager.CheckPasswordAsync(existingUser, password))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingUser);
                EnsureSuccess(await userManager.ResetPasswordAsync(existingUser, resetToken, password));
            }
            else if (!await userManager.HasPasswordAsync(existingUser))
            {
                EnsureSuccess(await userManager.AddPasswordAsync(existingUser, password));
            }

            return existingUser;
        }

        var user = new User
        {
            Id = AuthId.New(),
            UserName = email,
            Email = email
        };

        EnsureSuccess(await userManager.CreateAsync(user, password));
        return user;
    }

    internal static async Task<User> EnsureUserWithRolesAsync(
        IServiceProvider serviceProvider,
        string email,
        string password,
        params string[] roles)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var user = await EnsureUserAsync(serviceProvider, email, password);
        var existingRoles = await userManager.GetRolesAsync(user);

        foreach (var role in roles.Distinct(StringComparer.Ordinal))
        {
            if (existingRoles.Contains(role, StringComparer.Ordinal))
            {
                continue;
            }

            EnsureSuccess(await userManager.AddToRoleAsync(user, role));
        }

        return user;
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(static error => error.Description));
        throw new InvalidOperationException($"Auth test data seeding failed: {errors}");
    }
}
