namespace Warehouse.Domain.Product.Exceptions;

internal class PriceLessThanZeroException : Exception
{
    public PriceLessThanZeroException(string? message) : base(message)
    {
    }
}
