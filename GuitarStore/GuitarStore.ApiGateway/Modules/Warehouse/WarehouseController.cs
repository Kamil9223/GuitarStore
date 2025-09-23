using Application.CQRS.Command;
using Application.CQRS.Query;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Warehouse;

[ApiController]
[Route("warehouse")]
public class WarehouseController : ControllerBase
{
    private readonly ICommandHandlerExecutor _commandHandlerExecutor;
    private readonly IQueryHandlerExecutor _queryHandlerExecutor;

    public WarehouseController(ICommandHandlerExecutor commandHandlerExecutor, IQueryHandlerExecutor queryHandlerExecutor)
    {
        _commandHandlerExecutor = commandHandlerExecutor;
        _queryHandlerExecutor = queryHandlerExecutor;
    }

    [HttpPatch]
    public async Task<IActionResult> IncreaseQuantity()
    {
        throw new NotImplementedException();
    }
}
