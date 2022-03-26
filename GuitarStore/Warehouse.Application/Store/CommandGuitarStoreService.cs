using System.Text;
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
        AddGuitarStoreCommandValidator validator = new AddGuitarStoreCommandValidator();

        var validationResult = validator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errorBuilder = new StringBuilder();

            errorBuilder.AppendLine("Invalid command, reason: ");

            foreach (var error in validationResult.Errors)
            {
                errorBuilder.AppendLine(error.ErrorMessage);
            }

            throw new Exception(errorBuilder.ToString());
        }

        await _guitarStoreRepository.Add(
            GuitarStore.Create(command.Name, StoreLocation.Create(command.Street, command.PostalCode, command.City))
            );
    }
}
