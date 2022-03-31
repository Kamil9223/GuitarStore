using Warehouse.Contracts.Store;
using Warehouse.Domain.Store;

namespace Warehouse.Application.Store;

internal class CommandGuitarStoreService : ICommandGuitarStoreService
{
    private readonly IGuitarStoreRepository _guitarStoreRepository;
    private readonly IValidationService<AddGuitarStoreCommand> _validationService;

    public CommandGuitarStoreService(
        IGuitarStoreRepository guitarStoreRepository,
        IValidationService<AddGuitarStoreCommand> validationService)
    {
        _guitarStoreRepository = guitarStoreRepository;
        _validationService = validationService;
    }

    public async Task AddGuitarStore(AddGuitarStoreCommand command)
    {
        _validationService.Validate(command);

        await _guitarStoreRepository.Add(
            GuitarStore.Create(command.Name, StoreLocation.Create(command.Street, command.PostalCode, command.City))
            );
    }
}
