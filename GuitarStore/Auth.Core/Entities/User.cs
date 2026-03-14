using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Entities;

public sealed class User : IdentityUser<AuthId>
{
}
