using Microsoft.AspNetCore.Mvc;
using Warehouse.Contracts;
using Warehouse.Contracts.Store;

namespace GuitarStore.ApiGateway.Modules.Warehouse.Store;

[ApiController]
[Route("stores")]
public class GuitarStoreController : ControllerBase
{
    ICommandHandlerExecutor<AddGuitarStoreCommand> _addGuitarStoreCommandExecutor;
    ICommandHandlerExecutor<UpdateGuitarStoreCommand> _updateGuitarStoreCommandExecutor;

    public GuitarStoreController(
        ICommandHandlerExecutor<AddGuitarStoreCommand> addGuitarStoreCommandExecutor,
        ICommandHandlerExecutor<UpdateGuitarStoreCommand> updateGuitarStoreCommandExecutor)
    {
        _addGuitarStoreCommandExecutor = addGuitarStoreCommandExecutor;
        _updateGuitarStoreCommandExecutor = updateGuitarStoreCommandExecutor;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddGuitarStoreRequest request)
    {
        await _addGuitarStoreCommandExecutor.Execute(new AddGuitarStoreCommand
        {
            Name = request.Name,
            City = request.City,
            PostalCode = request.PostalCode,
            Street = request.Street
        });

        return Ok();
    }

    [HttpPut("{guitarStoreId}")]
    public async Task<IActionResult> Update(int guitarStoreId, UpdateGuitarStoreRequest request)
    {
        await _updateGuitarStoreCommandExecutor.Execute(new UpdateGuitarStoreCommand
        {
            Id = guitarStoreId,
            Name = request.Name,
            City = request.City,
            PostalCode = request.PostalCode,
            Street= request.Street
        });

        return Ok();
    }
}
