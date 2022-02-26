namespace Warehouse.Contracts.Store;

public interface ICommandGuitarStoreService
{
    Task AddGuitarStore(AddGuitarStoreCommand command);
}
