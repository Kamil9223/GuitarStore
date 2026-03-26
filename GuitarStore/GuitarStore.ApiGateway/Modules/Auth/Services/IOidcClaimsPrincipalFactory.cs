using Auth.Core.Entities;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace GuitarStore.ApiGateway.Modules.Auth.Services;

public interface IOidcClaimsPrincipalFactory
{
    Task<ClaimsPrincipal> CreateAsync(User user, OpenIddictRequest request);
}
