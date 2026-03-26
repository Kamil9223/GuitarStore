using System.Text.RegularExpressions;

namespace Tests.EndToEnd.E2E_Auth;

internal static partial class AuthUiTestHelpers
{
    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex RequestVerificationTokenRegex();

    internal static string ExtractAntiForgeryToken(string html)
    {
        var match = RequestVerificationTokenRegex().Match(html);
        if (!match.Success)
        {
            throw new InvalidOperationException("Anti-forgery token was not found in the HTML response.");
        }

        return match.Groups[1].Value;
    }
}
