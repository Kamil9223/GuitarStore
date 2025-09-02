using Common.Errors;
using Common.Errors.Exceptions;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

public class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new Regex(
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Email { get; }

    private EmailAddress(string email)
    {
        Email = email;
    }

    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
        {
            throw DomainException.InvalidEmailAddress(email);
        }

        return new EmailAddress(email);
    }

    public static bool operator ==(EmailAddress emailAddress, string email)
        => emailAddress.Email == email;

    public static bool operator !=(EmailAddress emailAddress, string email)
        => emailAddress.Email != email;
}
