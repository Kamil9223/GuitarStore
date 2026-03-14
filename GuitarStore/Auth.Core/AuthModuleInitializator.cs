using Auth.Core.Configuration;
using Auth.Core.Data;
using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Core;

public static class AuthModuleInitializator
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));

        var authOptions = configuration.GetRequiredSection(AuthOptions.SectionName).Get<AuthOptions>();
        var issuer = configuration.GetValue<string>($"{AuthOptions.SectionName}:{nameof(AuthOptions.Issuer)}");
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("Auth issuer must be configured.");
        }

        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseOpenIddict();
        });

        services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedEmail = authOptions!.RequireEmailConfirmed;
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

        services.AddAuthorization();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthDbContext>();
            })
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(issuer));
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetTokenEndpointUris("/connect/token");
                options.SetEndSessionEndpointUris("/connect/logout");

                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();
                options.AllowRefreshTokenFlow();

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(authOptions!.AccessTokenMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(authOptions.RefreshTokenDays));

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}
