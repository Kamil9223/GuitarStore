using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Configuration;

internal sealed class IdentityAdminInitializationStore(
    UserManager<User> userManager,
    RoleManager<Role> roleManager) : IAdminInitializationStore
{
    public Task<bool> RoleExistsAsync(string roleName) => roleManager.RoleExistsAsync(roleName);

    public async Task<bool> AnyUsersInRoleAsync(string roleName)
    {
        var users = await userManager.GetUsersInRoleAsync(roleName);
        return users.Count > 0;
    }

    public Task<User?> FindByEmailAsync(string email) => userManager.FindByEmailAsync(email);

    public Task<bool> IsInRoleAsync(User user, string roleName) => userManager.IsInRoleAsync(user, roleName);

    public async Task CreateAsync(User user, string password)
    {
        EnsureSuccess(
            await userManager.CreateAsync(user, password),
            $"creating admin user '{user.Email}'");
    }

    public async Task DeleteAsync(User user)
    {
        await userManager.DeleteAsync(user);
    }

    public async Task AddToRoleAsync(User user, string roleName)
    {
        EnsureSuccess(
            await userManager.AddToRoleAsync(user, roleName),
            $"assigning role '{roleName}' to user '{user.Email}'");
    }

    public async Task<IReadOnlyCollection<string>> ValidatePasswordAsync(User user, string password)
    {
        var errors = new List<string>();
        foreach (var validator in userManager.PasswordValidators)
        {
            var validationResult = await validator.ValidateAsync(userManager, user, password);
            if (validationResult.Succeeded)
            {
                continue;
            }

            errors.AddRange(validationResult.Errors.Select(static error => error.Description));
        }

        return errors;
    }

    private static void EnsureSuccess(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(static error => error.Description));
        throw new InvalidOperationException($"Admin initialization failed while {operation}: {errors}");
    }
}
