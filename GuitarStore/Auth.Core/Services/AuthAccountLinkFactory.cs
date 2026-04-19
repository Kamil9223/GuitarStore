using Auth.Core.Configuration;
using Auth.Core.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text;

namespace Auth.Core.Services;

internal sealed class AuthAccountLinkFactory(IOptions<AuthOptions> authOptions) : IAuthAccountLinkFactory
{
    private readonly Uri _baseUri = new(authOptions.Value.Issuer, UriKind.Absolute);

    public Uri CreateEmailConfirmationLink(User user, string token)
    {
        return BuildUri(
            "/auth/confirm-email",
            user,
            token);
    }

    public Uri CreatePasswordResetLink(User user, string token)
    {
        return BuildUri(
            "/auth/reset-password",
            user,
            token);
    }

    public string DecodeToken(string encodedToken)
    {
        var tokenBytes = WebEncoders.Base64UrlDecode(encodedToken);
        return Encoding.UTF8.GetString(tokenBytes);
    }

    private Uri BuildUri(string path, User user, string token)
    {
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var relativeUri = QueryHelpers.AddQueryString(path, new Dictionary<string, string?>
        {
            ["userId"] = user.Id.ToString(),
            ["token"] = encodedToken
        });

        return new Uri(_baseUri, relativeUri);
    }
}
