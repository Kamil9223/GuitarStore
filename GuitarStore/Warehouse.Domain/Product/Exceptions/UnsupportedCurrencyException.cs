namespace Warehouse.Domain.Product.Exceptions;

internal class UnsupportedCurrencyException : Exception
{
    public UnsupportedCurrencyException(string? message) : base(message)
    {
    }
}
