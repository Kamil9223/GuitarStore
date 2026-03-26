using Auth.Core.Configuration;
using Auth.Core.Data;
using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Core;

public static class AuthModuleInitializator
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));

        var authOptions = GetAuthOptions(configuration);
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;
        ConfigurePersistence(services, connectionString);
        ConfigureIdentity(services, authOptions);
        ConfigureAuthentication(services);
        ConfigureAuthorization(services);
        ConfigureOpenIddict(services, authOptions);

        return services;
    }

    private static AuthOptions GetAuthOptions(IConfiguration configuration)
    {
        var authOptions = configuration.GetRequiredSection(AuthOptions.SectionName).Get<AuthOptions>()
            ?? throw new InvalidOperationException("Auth options must be configured.");

        if (!Uri.TryCreate(authOptions.Issuer, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Auth issuer must be a valid absolute URI.");
        }

        if (authOptions.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException("Auth access token lifetime must be greater than zero.");
        }

        if (authOptions.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("Auth refresh token lifetime must be greater than zero.");
        }

        return authOptions;
    }

    private static void ConfigurePersistence(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseOpenIddict();
        });
    }

    private static void ConfigureIdentity(IServiceCollection services, AuthOptions authOptions)
    {
        services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedEmail = authOptions.RequireEmailConfirmed;
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = authOptions.Password.RequiredLength;
                options.Password.RequireDigit = authOptions.Password.RequireDigit;
                options.Password.RequireLowercase = authOptions.Password.RequireLowercase;
                options.Password.RequireUppercase = authOptions.Password.RequireUppercase;
                options.Password.RequireNonAlphanumeric = authOptions.Password.RequireNonAlphanumeric;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = authOptions.Lockout.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(authOptions.Lockout.DefaultLockoutMinutes);
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/auth/login";
            options.LogoutPath = "/auth/logout";
            options.AccessDeniedPath = "/auth/forbidden";
        });
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
            });
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddAuthorization();
    }

    private static void ConfigureOpenIddict(IServiceCollection services, AuthOptions authOptions)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthDbContext>();
            })
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(authOptions.Issuer));
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetTokenEndpointUris("/connect/token");
                options.SetEndSessionEndpointUris("/connect/logout");

                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();
                options.AllowRefreshTokenFlow();
                options.DisableAccessTokenEncryption();

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(authOptions.AccessTokenMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(authOptions.RefreshTokenDays));
                options.RegisterScopes(GetScopes(authOptions));

                ConfigureCertificates(options, authOptions);

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.SetIssuer(new Uri(authOptions.Issuer));
                options.UseLocalServer();
                options.UseAspNetCore();
                options.EnableTokenEntryValidation();
            });
    }

    private static string[] GetScopes(AuthOptions authOptions)
    {
        var scopes = new List<string>
        {
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.OfflineAccess
        };

        if (authOptions.Scopes.IncludeProfileScope)
        {
            scopes.Add(OpenIddictConstants.Scopes.Profile);
        }

        return scopes.ToArray();
    }

    private static void ConfigureCertificates(OpenIddictServerBuilder options, AuthOptions authOptions)
    {
        if (authOptions.Certificates.UseDevelopmentCertificates)
        {
            options.AddDevelopmentEncryptionCertificate()
                .AddDevelopmentSigningCertificate();

            return;
        }

        if (authOptions.Certificates.Encryption is null || authOptions.Certificates.Signing is null)
        {
            throw new InvalidOperationException("Production OpenIddict certificates must define both signing and encryption settings.");
        }

        options.AddEncryptionCertificate(LoadCertificate(authOptions.Certificates.Encryption, "encryption"));
        options.AddSigningCertificate(LoadCertificate(authOptions.Certificates.Signing, "signing"));
    }

    private static X509Certificate2 LoadCertificate(AuthOptions.CertificateDescriptor descriptor, string usage)
    {
        if (!string.IsNullOrWhiteSpace(descriptor.Path))
        {
            return string.IsNullOrWhiteSpace(descriptor.Password)
                ? new X509Certificate2(descriptor.Path)
                : new X509Certificate2(
                    descriptor.Path,
                    descriptor.Password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
        }

        if (!string.IsNullOrWhiteSpace(descriptor.Thumbprint))
        {
            using var store = new X509Store(descriptor.StoreName, descriptor.StoreLocation);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(
                X509FindType.FindByThumbprint,
                descriptor.Thumbprint,
                validOnly: false);

            if (certificates.Count > 0)
            {
                return certificates[0];
            }
        }

        throw new InvalidOperationException($"OpenIddict {usage} certificate must be loaded from a PFX path or a certificate store thumbprint.");
    }
}
