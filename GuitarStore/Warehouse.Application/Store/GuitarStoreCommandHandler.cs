using Warehouse.Contracts;
using Warehouse.Contracts.Store;
using Warehouse.Domain.Store;

namespace Warehouse.Application.Store;

internal class GuitarStoreCommandHandler :
    ICommandHandler<AddGuitarStoreCommand>,
    ICommandHandler<UpdateGuitarStoreCommand>,
    ICommandHandler<DeleteGuitarStoreCommand>
{
    private readonly IGuitarStoreRepository _guitarStoreRepository;

    public GuitarStoreCommandHandler(
        IGuitarStoreRepository guitarStoreRepository)
    {
        _guitarStoreRepository = guitarStoreRepository;
    }

    public async Task Handle(AddGuitarStoreCommand command)
    {
        //await _guitarStoreRepository.Add(GuitarStore.Create(command.Name, StoreLocation.Create(command.Street, command.PostalCode, command.City)));
    }

    public async Task Handle(UpdateGuitarStoreCommand command)
    {
        var guitarStore = await _guitarStoreRepository.Get(command.Id);

        //guitarStore.ChangeLocation(StoreLocation.Create(command.Street, command.PostalCode, command.City));
    }

    public async Task Handle(DeleteGuitarStoreCommand command)
    {
        var guitarStore = await _guitarStoreRepository.Get(command.Id);

        await _guitarStoreRepository.Remove(guitarStore);
    }
}
