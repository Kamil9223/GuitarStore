using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GuitarStore.ApiGateway.Modules.Auth.Services;

internal sealed class OidcClaimsPrincipalFactory(
    SignInManager<User> signInManager,
    UserManager<User> userManager) : IOidcClaimsPrincipalFactory
{
    public async Task<ClaimsPrincipal> CreateAsync(User user, OpenIddictRequest request)
    {
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        var identity = (ClaimsIdentity?)principal.Identity
            ?? throw new InvalidOperationException("User principal must expose a claims identity.");

        SetClaim(identity, Claims.Subject, await userManager.GetUserIdAsync(user));
        SetClaim(identity, Claims.Email, user.Email);

        var displayName = user.UserName ?? user.Email ?? await userManager.GetUserIdAsync(user);
        SetClaim(identity, Claims.Name, displayName);
        SetClaim(identity, Claims.PreferredUsername, displayName);

        principal.SetScopes(request.GetScopes());
        principal.SetDestinations(GetDestinations);

        return principal;
    }

    private static void SetClaim(ClaimsIdentity identity, string claimType, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var existingClaim = identity.FindFirst(claimType);
        if (existingClaim is not null)
        {
            if (existingClaim.Value == value)
            {
                return;
            }

            identity.RemoveClaim(existingClaim);
        }

        identity.AddClaim(new Claim(claimType, value));
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Subject:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            case Claims.Name:
            case Claims.PreferredUsername:
            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Profile) == true)
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                yield break;

            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
