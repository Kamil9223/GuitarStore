using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.Core.Configuration;

internal sealed class OpenIddictApplicationsInitializer(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<AuthOptions> authOptions,
    ILogger<OpenIddictApplicationsInitializer> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var options = authOptions.Value;

        foreach (var client in options.Clients)
        {
            var descriptor = BuildDescriptor(client, options.Scopes.IncludeProfileScope);
            var existingApplication = await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken);

            if (existingApplication is null)
            {
                await applicationManager.CreateAsync(descriptor, cancellationToken);
                logger.LogInformation("Registered OpenIddict public client '{ClientId}'.", client.ClientId);
                continue;
            }

            await applicationManager.UpdateAsync(existingApplication, descriptor, cancellationToken);
            logger.LogInformation("Updated OpenIddict public client '{ClientId}'.", client.ClientId);
        }
    }

    private static OpenIddictApplicationDescriptor BuildDescriptor(
        AuthOptions.ClientConfiguration client,
        bool includeProfileScope)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit,
            DisplayName = string.IsNullOrWhiteSpace(client.DisplayName) ? client.ClientId : client.DisplayName
        };

        foreach (var redirectUri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri));
        }

        foreach (var postLogoutRedirectUri in client.PostLogoutRedirectUris)
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
            Requirements.Features.ProofKeyForCodeExchange
        ]);

        if (includeProfileScope)
        {
            descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.Profile);
        }

        return descriptor;
    }
}
