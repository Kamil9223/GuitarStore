using Microsoft.AspNetCore.Mvc;
using Warehouse.Contracts.Store;

namespace GuitarStore.ApiGateway.Modules.Warehouse.Store;

[ApiController]
[Route("[controller]")]
public class GuitarStoreController : ControllerBase
{
    private readonly ICommandGuitarStoreService _commandGuitarStoreService;

    public GuitarStoreController(ICommandGuitarStoreService commandGuitarStoreService)
    {
        _commandGuitarStoreService = commandGuitarStoreService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddGuitarStoreRequest request)
    {
        await _commandGuitarStoreService.AddGuitarStore(new AddGuitarStoreCommand());

        return Ok();
    }
}
