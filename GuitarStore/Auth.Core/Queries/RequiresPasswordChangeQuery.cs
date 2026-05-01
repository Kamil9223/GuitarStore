using Application.CQRS.Query;
using Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth.Core.Queries;

internal sealed record RequiresPasswordChangeQuery(ClaimsPrincipal Principal) : IQuery;

internal sealed record RequiresPasswordChangeQueryResult(bool IsRequired);

internal sealed class RequiresPasswordChangeQueryHandler(
    UserManager<User> userManager) : IQueryHandler<RequiresPasswordChangeQuery, RequiresPasswordChangeQueryResult>
{
    public async Task<RequiresPasswordChangeQueryResult> Handle(RequiresPasswordChangeQuery query, CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(query.Principal);
        return new RequiresPasswordChangeQueryResult(user?.MustChangePassword == true);
    }
}
