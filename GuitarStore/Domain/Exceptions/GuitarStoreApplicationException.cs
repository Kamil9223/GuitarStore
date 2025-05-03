namespace Domain.Exceptions;

public abstract class GuitarStoreApplicationException : Exception
{
    public string ErrorCode { get; }
    public GuitarStoreApplicationException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
