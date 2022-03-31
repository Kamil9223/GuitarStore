using Microsoft.AspNetCore.Mvc;
using Warehouse.Contracts.Store;

namespace GuitarStore.ApiGateway.Modules.Warehouse.Store;

[ApiController]
[Route("stores")]
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
        //Log info about client request (or in middleware)
        await _commandGuitarStoreService.AddGuitarStore(new AddGuitarStoreCommand
        {
            Name = request.Name,
            City = request.City,
            PostalCode = request.PostalCode,
            Street = request.Street
        });

        return Ok();
    }
}
