using System.ComponentModel.DataAnnotations;
using Auth.Core.Authorization;
using Auth.Core.Entities;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth.Core.Configuration;

internal sealed class AdminInitializer(
    IAdminInitializationStore adminInitializationStore,
    IOptions<SeedAdminOptions> seedAdminOptions,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    ILogger<AdminInitializer> logger)
{
    private const string BootstrapSecretConfigurationKey = "ADMIN_BOOTSTRAP_SECRET";

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsDevelopmentLikeEnvironment())
        {
            await SeedDevelopmentAdminAsync(cancellationToken);
            return;
        }

        await SeedProductionAdminAsync(cancellationToken);
    }

    private async Task SeedDevelopmentAdminAsync(CancellationToken cancellationToken)
    {
        var options = seedAdminOptions.Value;
        if (!options.Enabled)
        {
            logger.LogInformation(
                "Admin seed is disabled for environment '{EnvironmentName}'.",
                hostEnvironment.EnvironmentName);
            return;
        }

        await ValidateSeedAdminConfigurationAsync(options, "Development admin seed");
        await EnsureAdminRoleExistsAsync();

        var user = await adminInitializationStore.FindByEmailAsync(options.Email);
        if (user is null)
        {
            user = await CreateAdminUserAsync(options, cancellationToken);
            logger.LogInformation(
                "Seeded development admin account in environment '{EnvironmentName}'.",
                hostEnvironment.EnvironmentName);
            return;
        }

        await EnsureAdminRoleAssignedAsync(user);
        logger.LogInformation(
            "Development admin seed found an existing account and ensured the admin role in environment '{EnvironmentName}'.",
            hostEnvironment.EnvironmentName);
    }

    private async Task SeedProductionAdminAsync(CancellationToken cancellationToken)
    {
        var options = seedAdminOptions.Value;
        var bootstrapEnabled = !string.IsNullOrWhiteSpace(configuration[BootstrapSecretConfigurationKey]);

        if (options.Enabled)
        {
            throw new InvalidOperationException(
                $"'{SeedAdminOptions.SectionName}:Enabled' can only be true in Development, Local or TestContainers environments.");
        }

        if (!bootstrapEnabled)
        {
            logger.LogInformation(
                "Production admin bootstrap skipped because '{BootstrapSecretConfigurationKey}' is not configured.",
                BootstrapSecretConfigurationKey);
            return;
        }

        await EnsureAdminRoleExistsAsync();

        if (await AnyAdminExistsAsync())
        {
            logger.LogInformation("Production admin bootstrap skipped because an admin account already exists.");
            return;
        }

        await ValidateSeedAdminConfigurationAsync(options, "Production admin bootstrap");

        var existingUser = await adminInitializationStore.FindByEmailAsync(options.Email);
        if (existingUser is not null)
        {
            if (await adminInitializationStore.IsInRoleAsync(existingUser, AuthRoles.Admin))
            {
                logger.LogInformation("Production admin bootstrap found the configured admin account already in the admin role.");
                return;
            }

            throw new InvalidOperationException(
                $"Production admin bootstrap cannot reuse existing user '{options.Email}' because the account already exists without the '{AuthRoles.Admin}' role.");
        }

        await CreateAdminUserAsync(options, cancellationToken);
        logger.LogInformation("Bootstrapped the first production admin account.");
    }

    private async Task<User> CreateAdminUserAsync(SeedAdminOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new User
        {
            Id = AuthId.New(),
            UserName = options.Email,
            Email = options.Email
        };

        await adminInitializationStore.CreateAsync(user, options.Password);

        try
        {
            await EnsureAdminRoleAssignedAsync(user);
            return user;
        }
        catch
        {
            await adminInitializationStore.DeleteAsync(user);
            throw;
        }
    }

    private async Task EnsureAdminRoleAssignedAsync(User user)
    {
        if (await adminInitializationStore.IsInRoleAsync(user, AuthRoles.Admin))
        {
            return;
        }

        await adminInitializationStore.AddToRoleAsync(user, AuthRoles.Admin);
    }

    private async Task EnsureAdminRoleExistsAsync()
    {
        if (await adminInitializationStore.RoleExistsAsync(AuthRoles.Admin))
        {
            return;
        }

        throw new InvalidOperationException($"Auth role '{AuthRoles.Admin}' must exist before seeding an admin account.");
    }

    private Task<bool> AnyAdminExistsAsync() => adminInitializationStore.AnyUsersInRoleAsync(AuthRoles.Admin);

    private async Task ValidateSeedAdminConfigurationAsync(SeedAdminOptions options, string scenarioName)
    {
        if (string.IsNullOrWhiteSpace(options.Email))
        {
            throw new InvalidOperationException($"{scenarioName} requires '{SeedAdminOptions.SectionName}:Email' to be configured.");
        }

        if (!new EmailAddressAttribute().IsValid(options.Email))
        {
            throw new InvalidOperationException($"{scenarioName} requires a valid email address in '{SeedAdminOptions.SectionName}:Email'.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException($"{scenarioName} requires '{SeedAdminOptions.SectionName}:Password' to be configured.");
        }

        var validationUser = new User
        {
            Id = AuthId.New(),
            UserName = options.Email,
            Email = options.Email
        };

        var passwordValidationErrors = await adminInitializationStore.ValidatePasswordAsync(validationUser, options.Password);

        if (passwordValidationErrors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{scenarioName} password does not satisfy the current Identity policy: {string.Join(", ", passwordValidationErrors.Distinct(StringComparer.Ordinal))}");
    }

    private bool IsDevelopmentLikeEnvironment()
    {
        return hostEnvironment.IsDevelopment()
            || hostEnvironment.IsEnvironment("Local")
            || hostEnvironment.IsEnvironment("TestContainers");
    }
}
