using Auth.Core.Entities;

namespace Auth.Core.Services;

public interface IAuthAccountLinkFactory
{
    Uri CreateEmailConfirmationLink(User user, string token);
    Uri CreatePasswordResetLink(User user, string token);
    string DecodeToken(string encodedToken);
}
