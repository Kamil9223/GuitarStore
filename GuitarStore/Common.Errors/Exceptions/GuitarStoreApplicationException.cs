namespace Common.Errors.Exceptions;

public abstract class GuitarStoreApplicationException : Exception
{
    public ApplicationErrorCode ErrorCode { get; }
    public string Title { get; }


    public GuitarStoreApplicationException(
        string title,
        ApplicationErrorCode applicationErrorCode)
        : base(applicationErrorCode)
    {
        Title = title;
        ErrorCode = applicationErrorCode;
    }
}
