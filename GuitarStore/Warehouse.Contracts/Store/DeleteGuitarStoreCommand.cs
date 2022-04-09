using Application;

namespace Warehouse.Contracts.Store;

public class DeleteGuitarStoreCommand : ICommand
{
    public int Id { get; init; }
}
