namespace Domain.Exceptions;

/// <summary>
/// Throws when schema validation is broken.
/// </summary>
public class ValidationException : GuitarStoreApplicationException
{
    public ValidationException(string message)
        : base(message, string.Empty) { }
}
