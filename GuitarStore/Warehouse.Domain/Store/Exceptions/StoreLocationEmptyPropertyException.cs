namespace Warehouse.Domain.Store.Exceptions;

internal class StoreLocationEmptyPropertyException : Exception
{
    public StoreLocationEmptyPropertyException(string message) : base(message)
    {

    }
}
