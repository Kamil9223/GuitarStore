using Application.CQRS;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Orders.Commands;

namespace GuitarStore.ApiGateway.Modules.Orders.Orders;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly ICommandHandlerExecutor _commandHandlerExecutor;
    private readonly IQueryHandlerExecutor _queryHandlerExecutor;

    public OrdersController(ICommandHandlerExecutor commandHandlerExecutor, IQueryHandlerExecutor queryHandlerExecutor)
    {
        _commandHandlerExecutor = commandHandlerExecutor;
        _queryHandlerExecutor = queryHandlerExecutor;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderCommand request)
    {
        var response = await _commandHandlerExecutor.Execute<string, PlaceOrderCommand>(request);

        return Ok(response);
    }
}
