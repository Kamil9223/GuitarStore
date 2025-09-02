using Domain.StronglyTypedIds.Helpers;

namespace Common.Errors.Exceptions;

public class DomainException : GuitarStoreApplicationException
{
    private const string ExceptionTitle = "Business_Logic_Rule_Error";

    public DomainException(ApplicationErrorCode errorCode)
        : base(
            title: ExceptionTitle,
            applicationErrorCode: errorCode)
    {
    }

    public DomainException(ApplicationErrorCode errorCode, params object[] values)
       : base(
           title: ExceptionTitle,
           applicationErrorCode: errorCode.Format(values))
    {
    }

    public static DomainException CannotDescreaseQuantity(int quantity, IStronglyTypedId Id)
        => new(ApplicationErrorCode.CannotDescreaseQuantity, quantity.ToString(), Id.ToString()!);

    public static DomainException InvalidProperty(string propertyName, string propertyValue)
        => new(ApplicationErrorCode.InvalidPropertyValue, propertyName, propertyValue);

    public static DomainException UnsupportedCurrency(string currencyCode)
        => new(ApplicationErrorCode.UnsupportedCurrency, currencyCode);

    public static DomainException InvalidEmailAddress(string emailAddress)
        => new(ApplicationErrorCode.InvalidEmailAddress, emailAddress);
}
