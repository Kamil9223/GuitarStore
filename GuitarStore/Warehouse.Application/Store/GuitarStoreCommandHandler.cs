using Warehouse.Application.Abstractions;
using Warehouse.Application.Store.Commands;
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
        var guitarStore = new GuitarStore(
            name: command.Name,
            street: command.Street,
            postalCode: command.PostalCode,
            city: command.City);

        await _guitarStoreRepository.Add(guitarStore);
    }

    public async Task Handle(UpdateGuitarStoreCommand command)
    {
        var guitarStore = await _guitarStoreRepository.Get(command.Id);

        guitarStore.ChangeLocation(command.Street, command.PostalCode, command.City);
    }

    public async Task Handle(DeleteGuitarStoreCommand command)
    {
        var guitarStore = await _guitarStoreRepository.Get(command.Id);

        await _guitarStoreRepository.Remove(guitarStore);
    }
}
