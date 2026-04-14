using Auth.Core.Authorization;
using Auth.Core.Configuration;
using Auth.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests.Unit.Auth;

public sealed class AdminInitializerTest
{
    [Fact]
    public async Task DevSeed_ShouldCreateConfiguredAdmin()
    {
        var email = $"auth-step7-dev-seed-{Guid.NewGuid():N}@guitarstore.local";
        var store = new FakeAdminInitializationStore();
        var sut = CreateSut(
            store,
            Environments.Development,
            new Dictionary<string, string?>
            {
                ["SeedAdmin:Enabled"] = "true",
                ["SeedAdmin:Email"] = email,
                ["SeedAdmin:Password"] = "ChangeMe!123"
            });

        await sut.SeedAsync(CancellationToken.None);

        var user = Assert.Single(store.Users);
        Assert.Equal(email, user.Email);
        Assert.False(user.MustChangePassword);
        Assert.True(store.IsUserInRole(email, AuthRoles.Admin));
    }

    [Fact]
    public async Task DevSeed_ShouldBeIdempotent()
    {
        var email = $"auth-step7-dev-idempotent-{Guid.NewGuid():N}@guitarstore.local";
        var store = new FakeAdminInitializationStore();
        var sut = CreateSut(
            store,
            Environments.Development,
            new Dictionary<string, string?>
            {
                ["SeedAdmin:Enabled"] = "true",
                ["SeedAdmin:Email"] = email,
                ["SeedAdmin:Password"] = "ChangeMe!123"
            });

        await sut.SeedAsync(CancellationToken.None);
        await sut.SeedAsync(CancellationToken.None);

        Assert.Single(store.Users);
        Assert.Equal(1, store.CreateCalls);
        Assert.False(Assert.Single(store.Users).MustChangePassword);
        Assert.True(store.IsUserInRole(email, AuthRoles.Admin));
    }

    [Fact]
    public async Task ProdBootstrap_WithoutGate_ShouldDoNothing()
    {
        var email = $"auth-step7-prod-no-gate-{Guid.NewGuid():N}@guitarstore.local";
        var store = new FakeAdminInitializationStore();
        var sut = CreateSut(
            store,
            Environments.Production,
            new Dictionary<string, string?>
            {
                ["SeedAdmin:Enabled"] = "false",
                ["SeedAdmin:Email"] = email,
                ["SeedAdmin:Password"] = "ChangeMe!123"
            });

        await sut.SeedAsync(CancellationToken.None);

        Assert.Empty(store.Users);
        Assert.False(store.AnyUsersInRole(AuthRoles.Admin));
    }

    [Fact]
    public async Task ProdBootstrap_WithGate_ShouldCreateFirstAdminOnlyOnce()
    {
        var email = $"auth-step7-prod-bootstrap-{Guid.NewGuid():N}@guitarstore.local";
        var store = new FakeAdminInitializationStore();
        var sut = CreateSut(
            store,
            Environments.Production,
            new Dictionary<string, string?>
            {
                ["SeedAdmin:Enabled"] = "false",
                ["SeedAdmin:Email"] = email,
                ["SeedAdmin:Password"] = "ChangeMe!123",
                ["ADMIN_BOOTSTRAP_SECRET"] = "bootstrap-secret"
            });

        await sut.SeedAsync(CancellationToken.None);
        await sut.SeedAsync(CancellationToken.None);

        Assert.Single(store.Users);
        Assert.Equal(1, store.CreateCalls);
        Assert.True(Assert.Single(store.Users).MustChangePassword);
        Assert.True(store.IsUserInRole(email, AuthRoles.Admin));
    }

    [Fact]
    public async Task InvalidSeedAdminConfiguration_ShouldFailFast()
    {
        var sut = CreateSut(
            new FakeAdminInitializationStore(),
            Environments.Development,
            new Dictionary<string, string?>
            {
                ["SeedAdmin:Enabled"] = "true",
                ["SeedAdmin:Email"] = string.Empty,
                ["SeedAdmin:Password"] = "ChangeMe!123"
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SeedAsync(CancellationToken.None));

        Assert.Contains("SeedAdmin:Email", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProdBootstrap_WhenConfiguredUserExistsWithoutAdminRole_ShouldFail()
    {
        var email = $"auth-step7-prod-existing-{Guid.NewGuid():N}@guitarstore.local";
        var store = new FakeAdminInitializationStore();
        store.SeedUser(new User
        {
            UserName = email,
            Email = email
        });

        var sut = CreateSut(
            store,
            Environments.Production,
            new Dictionary<string, string?>
            {
                ["SeedAdmin:Enabled"] = "false",
                ["SeedAdmin:Email"] = email,
                ["SeedAdmin:Password"] = "ChangeMe!123",
                ["ADMIN_BOOTSTRAP_SECRET"] = "bootstrap-secret"
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SeedAsync(CancellationToken.None));

        Assert.Contains(email, exception.Message, StringComparison.Ordinal);
        Assert.Contains(AuthRoles.Admin, exception.Message, StringComparison.Ordinal);
    }

    private static AdminInitializer CreateSut(
        FakeAdminInitializationStore store,
        string environmentName,
        IReadOnlyDictionary<string, string?> overrides)
    {
        var configuration = BuildConfiguration(overrides);
        var options = new SeedAdminOptions
        {
            Enabled = bool.TryParse(configuration["SeedAdmin:Enabled"], out var enabled) && enabled,
            Email = configuration["SeedAdmin:Email"] ?? string.Empty,
            Password = configuration["SeedAdmin:Password"] ?? string.Empty
        };

        return new AdminInitializer(
            store,
            Options.Create(options),
            configuration,
            new TestHostEnvironment(environmentName),
            NullLogger<AdminInitializer>.Instance);
    }

    private static IConfiguration BuildConfiguration(IReadOnlyDictionary<string, string?> overrides)
    {
        var settings = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["SeedAdmin:Enabled"] = "false",
            ["SeedAdmin:Email"] = "admin@guitarstore.local",
            ["SeedAdmin:Password"] = "ChangeMe!123"
        };

        foreach (var entry in overrides)
        {
            settings[entry.Key] = entry.Value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = nameof(AdminInitializerTest);
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class FakeAdminInitializationStore : IAdminInitializationStore
    {
        private readonly Dictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> _rolesByEmail = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _existingRoles = new(StringComparer.Ordinal)
        {
            AuthRoles.Admin
        };

        private readonly List<string> _passwordValidationErrors = [];

        public int CreateCalls { get; private set; }

        public IReadOnlyCollection<User> Users => _usersByEmail.Values.ToArray();

        public Task<bool> RoleExistsAsync(string roleName) => Task.FromResult(_existingRoles.Contains(roleName));

        public Task<bool> AnyUsersInRoleAsync(string roleName) => Task.FromResult(AnyUsersInRole(roleName));

        public Task<User?> FindByEmailAsync(string email)
        {
            _usersByEmail.TryGetValue(email, out var user);
            return Task.FromResult(user);
        }

        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            var email = user.Email ?? string.Empty;
            return Task.FromResult(IsUserInRole(email, roleName));
        }

        public Task CreateAsync(User user, string password)
        {
            var email = user.Email ?? throw new InvalidOperationException("User email must be set.");
            if (_usersByEmail.ContainsKey(email))
            {
                throw new InvalidOperationException($"User '{email}' already exists.");
            }

            _usersByEmail[email] = user;
            _rolesByEmail[email] = new HashSet<string>(StringComparer.Ordinal);
            CreateCalls++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(User user)
        {
            var email = user.Email ?? string.Empty;
            _usersByEmail.Remove(email);
            _rolesByEmail.Remove(email);
            return Task.CompletedTask;
        }

        public Task AddToRoleAsync(User user, string roleName)
        {
            var email = user.Email ?? throw new InvalidOperationException("User email must be set.");
            if (!_usersByEmail.ContainsKey(email))
            {
                throw new InvalidOperationException($"User '{email}' does not exist.");
            }

            if (!_rolesByEmail.TryGetValue(email, out var roles))
            {
                roles = new HashSet<string>(StringComparer.Ordinal);
                _rolesByEmail[email] = roles;
            }

            roles.Add(roleName);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<string>> ValidatePasswordAsync(User user, string password)
        {
            return Task.FromResult<IReadOnlyCollection<string>>(_passwordValidationErrors.ToArray());
        }

        public void SeedUser(User user, params string[] roles)
        {
            var email = user.Email ?? throw new InvalidOperationException("User email must be set.");
            _usersByEmail[email] = user;
            _rolesByEmail[email] = new HashSet<string>(roles, StringComparer.Ordinal);
        }

        public bool AnyUsersInRole(string roleName)
        {
            return _rolesByEmail.Values.Any(roles => roles.Contains(roleName));
        }

        public bool IsUserInRole(string email, string roleName)
        {
            return _rolesByEmail.TryGetValue(email, out var roles) && roles.Contains(roleName);
        }
    }
}
