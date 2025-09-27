using System.Globalization;

namespace Common.Errors;

public class ApplicationErrorCode
{
    private readonly string _message;

    private ApplicationErrorCode(string errorMessage)
    {
        _message = errorMessage;
    }

    public static implicit operator string(ApplicationErrorCode ErrorCode) => ErrorCode._message;
    public static implicit operator ApplicationErrorCode(string ErrorMessage) => new(ErrorMessage);

    /// <summary>
    /// Returns formatted error message using passed arguments.
    /// </summary>
    public ApplicationErrorCode Format(params object[] args) => new(string.Format(CultureInfo.InvariantCulture, _message, args));

    public static ApplicationErrorCode SchemaValidationError(string message) => new(message);
    public static ApplicationErrorCode ResourceNotFound => new("Resource with Id: [{0}] does not exist.");
    public static ApplicationErrorCode UnsupportedCurrency => new("Unsupported currency code: [{0}]");
    public static ApplicationErrorCode InvalidEmailAddress => new("Invalid email address: [{0}]");
    public static ApplicationErrorCode InvalidPropertyValue => new("[{0}] invalid. Invalid value: [{1}]");
    public static ApplicationErrorCode CannotDescreaseQuantity => new("Cannot decrease quantity: [{0}] of productId: [{1}]");
}
