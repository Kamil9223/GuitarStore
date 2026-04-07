using Auth.Core.Entities;

namespace Auth.Core.Configuration;

internal interface IAdminInitializationStore
{
    Task<bool> RoleExistsAsync(string roleName);
    Task<bool> AnyUsersInRoleAsync(string roleName);
    Task<User?> FindByEmailAsync(string email);
    Task<bool> IsInRoleAsync(User user, string roleName);
    Task CreateAsync(User user, string password);
    Task DeleteAsync(User user);
    Task AddToRoleAsync(User user, string roleName);
    Task<IReadOnlyCollection<string>> ValidatePasswordAsync(User user, string password);
}
