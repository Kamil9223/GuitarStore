using Warehouse.Contracts.Store;
using Warehouse.Domain.Store;

namespace Warehouse.Application.Store;

internal class CommandGuitarStoreService : ICommandGuitarStoreService
{
    private readonly IGuitarStoreRepository _guitarStoreRepository;

    public CommandGuitarStoreService(IGuitarStoreRepository guitarStoreRepository)
    {
        _guitarStoreRepository = guitarStoreRepository;
    }

    public async Task AddGuitarStore(AddGuitarStoreCommand command)
    {
        await _guitarStoreRepository.Add(GuitarStore.Create("test", null));
    }
}
