using Warehouse.Contracts;
using Warehouse.Contracts.Store;
using Warehouse.Domain.Store;

namespace Warehouse.Application.Store;

internal class GuitarStoreCommandHandler :
    ICommandHandler<AddGuitarStoreCommand>,
    ICommandHandler<UpdateGuitarStoreCommand>
{
    private readonly IGuitarStoreRepository _guitarStoreRepository;

    public GuitarStoreCommandHandler(
        IGuitarStoreRepository guitarStoreRepository)
    {
        _guitarStoreRepository = guitarStoreRepository;
    }

    public Task Handle(AddGuitarStoreCommand command)
    {
        return Task.CompletedTask;
        //await _guitarStoreRepository.Add(
        //    GuitarStore.Create(command.Name, StoreLocation.Create(command.Street, command.PostalCode, command.City))
        //    );
    }

    public Task Handle(UpdateGuitarStoreCommand command)
    {
        return Task.CompletedTask;
    }
}
