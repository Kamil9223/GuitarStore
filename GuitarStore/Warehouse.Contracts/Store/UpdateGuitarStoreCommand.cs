using Application;

namespace Warehouse.Contracts.Store;

public class UpdateGuitarStoreCommand : ICommand
{
    public int Id { get; init; }

    public string Name { get; init; }

    public string City { get; init; }

    public string Street { get; init; }

    public string PostalCode { get; init; }
}
