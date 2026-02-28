namespace Common.Errors.Exceptions;

public class StripeSignatureException : GuitarStoreApplicationException
{
    private const string ExceptionTitle = "Stripe_Signature_Invalid";
    
    public StripeSignatureException(ApplicationErrorCode errorCode)
        : base(
            title: ExceptionTitle,
            applicationErrorCode: errorCode)
    { }
    
    public StripeSignatureException(ApplicationErrorCode errorCode, params object[] values)
        : base(
            title: ExceptionTitle,
            applicationErrorCode: errorCode.Format(values))
    { }
}