using Application.CQRS.Command;
using Application.CQRS.Query;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Core.Commands;

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

    [HttpPut("increase-quantity", Name = "IncreaseProductQuantity")]
    public async Task<IActionResult> IncreaseQuantity(IncreaseStockQuantityCommand command)
    {
        await _commandHandlerExecutor.Execute(command);
        return Ok();
    }
}
