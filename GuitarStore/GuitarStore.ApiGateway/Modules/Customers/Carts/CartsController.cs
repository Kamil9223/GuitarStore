using Application.CQRS.Command;
using Application.CQRS.Query;
using Customers.Application.Carts.Commands;
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
    public async Task<IActionResult> Create(AddCartItemCommand request)
    {
        await _commandHandlerExecutor.Execute(request);

        return Ok();
    }

    [HttpPost("checkout", Name = "CheckoutCart")]
    public async Task<IActionResult> Checkout(CheckoutCartCommand request)
    {
        await _commandHandlerExecutor.Execute(request);

        return Ok();
    }

    [HttpDelete("RemoveItemFromCart")]
    public async Task<IActionResult> RemoveFromCart()
    {
        throw new NotImplementedException();
    }

    [HttpGet(Name = "GetCart")]
    public async Task<IActionResult> GetCart()
    {
        throw new NotImplementedException();
    }
}
