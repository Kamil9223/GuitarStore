using Auth.Core.Entities;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

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

    internal static async Task EnsureOidcPublicClientAsync(
        IServiceProvider serviceProvider,
        string clientId,
        string redirectUri,
        string? postLogoutRedirectUri = null)
    {
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        if (await applicationManager.FindByClientIdAsync(clientId) is not null)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit,
            DisplayName = "Tests.EndToEnd SPA client"
        };

        descriptor.RedirectUris.Add(new Uri(redirectUri));

        if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri))
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
        }

        descriptor.Permissions.UnionWith(
        [
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.RefreshToken,
            Permissions.ResponseTypes.Code,
            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.OfflineAccess,
            Permissions.Prefixes.Scope + Scopes.Profile,
            Requirements.Features.ProofKeyForCodeExchange
        ]);

        await applicationManager.CreateAsync(descriptor);
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
