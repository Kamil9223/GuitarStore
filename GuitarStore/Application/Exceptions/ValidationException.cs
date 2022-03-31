namespace Application.Exceptions;

/// <summary>
/// Throws when schema validation is broken.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
