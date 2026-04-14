using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;

namespace Auth.Core.Entities;

public sealed class User : IdentityUser<AuthId>
{
    public bool MustChangePassword { get; private set; }

    public void RequirePasswordChange()
    {
        MustChangePassword = true;
    }

    public void MarkPasswordChanged()
    {
        MustChangePassword = false;
    }
}
