using Application.CQRS.Command;
using Application.CQRS.Query;
using Customers.Application.Carts.Commands;
using Customers.Application.Carts.Queries;
using Domain.StronglyTypedIds;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Customers.Carts;

[ApiController]
[Route("carts")]
public class CartsController : ControllerBase
{
    private readonly ICommandHandlerExecutor _commandHandlerExecutor;
    private readonly IQueryHandlerExecutor _queryHandlerExecutor;

    public CartsController(ICommandHandlerExecutor commandHandlerExecutor, IQueryHandlerExecutor queryHandlerExecutor)
    {
        _commandHandlerExecutor = commandHandlerExecutor;
        _queryHandlerExecutor = queryHandlerExecutor;
    }

    [HttpPost(Name = "AddItemToCart")]
    public async Task<IActionResult> Create(AddCartItemCommand request, CancellationToken ct)
    {
        await _commandHandlerExecutor.Execute(request, ct);

        return Ok();
    }

    [HttpPost("checkout", Name = "CheckoutCart")]
    public async Task<IActionResult> Checkout(CheckoutCartCommand request, CancellationToken ct)
    {
        await _commandHandlerExecutor.Execute(request, ct);

        return Ok();
    }

    [HttpDelete("{productId}", Name = "RemoveItemFromCart")]
    public async Task<IActionResult> RemoveFromCart(ProductId productId, CancellationToken ct)
    {
        await _commandHandlerExecutor.Execute(new RemoveCartItemCommand(CustomerId.New(), productId), ct);

        return Ok();
    }

    [HttpGet(Name = "GetCart")]
    public async Task<ActionResult<CartDetailsResponse>> GetCart(CancellationToken ct)
    {
        var response = await _queryHandlerExecutor
            .Execute<GetCartQuery, CartDetailsResponse>(new GetCartQuery(CustomerId.New()), ct);

        return Ok(response);
    }
}
