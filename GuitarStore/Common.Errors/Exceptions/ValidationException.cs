namespace Common.Errors.Exceptions;

/// <summary>
/// Throws when schema validation is broken.
/// </summary>
public class ValidationException : GuitarStoreApplicationException
{
    public ValidationException(string message)
        : base(
            title: "Bad_Request",
            applicationErrorCode: ApplicationErrorCode.SchemaValidationError(message))
    { }
}
