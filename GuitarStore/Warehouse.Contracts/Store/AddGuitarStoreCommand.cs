namespace Warehouse.Contracts.Store;

public class AddGuitarStoreCommand
{
    public string Name { get; init; }

    public string City { get; init; }

    public string Street { get; init; }

    public string PostalCode { get; init; }
}
