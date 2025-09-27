using Application.CQRS.Command;
using Application.CQRS.Query;
using Domain.StronglyTypedIds;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Queries;

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

    [HttpPost(Name = "PlaceOrder")]
    public async Task<ActionResult<PlaceOrderResponse>> PlaceOrder(PlaceOrderCommand request)
    {
        var response = await _commandHandlerExecutor.Execute<PlaceOrderResponse, PlaceOrderCommand>(request);

        return Ok(response);
    }

    [HttpGet("{orderId}/status", Name = "CheckOrderStatus")]
    public async Task<ActionResult<OrderStatusResponse>> CheckOrderStatus([FromRoute] OrderId orderId)
    {
        var response = await _queryHandlerExecutor.Execute<GetOrderStatusQuery, OrderStatusResponse>(new GetOrderStatusQuery(orderId));

        return Ok(response);
    }

    [HttpGet("OrderHistory/{customerId}")]//TODO: pobierać z headera
    public async Task<ActionResult<OrdersHistoryResponse>> GetOrdersHistory([FromRoute] CustomerId customerId)
    {
        var response = await _queryHandlerExecutor.Execute<GetOrdersHistoryQuery, OrdersHistoryResponse>(new GetOrdersHistoryQuery(customerId));

        return Ok(response);
    }

    [HttpGet("{orderId}", Name = "OrderDetails")]
    public async Task<ActionResult<OrderDetailsResponse>> GetOrderDetails([FromRoute] OrderId orderId)
    {
        var response = await _queryHandlerExecutor.Execute<GetOrderDetailsQuery, OrderDetailsResponse>(new GetOrderDetailsQuery(orderId));

        return Ok(response);
    }
}
