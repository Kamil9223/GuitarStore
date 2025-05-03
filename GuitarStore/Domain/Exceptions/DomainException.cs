namespace Domain.Exceptions;

public class DomainException : GuitarStoreApplicationException
{
    public DomainException(string? message) : base(message, string.Empty)
    {
    }
}
