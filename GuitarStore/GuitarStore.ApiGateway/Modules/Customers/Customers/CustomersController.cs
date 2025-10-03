using Application.CQRS.Command;
using Customers.Application.Customers.Commands;
using Microsoft.AspNetCore.Mvc;

namespace GuitarStore.ApiGateway.Modules.Customers.Customers;

[ApiController]
[Route("customers")]
public class CustomersController : ControllerBase
{
    private readonly ICommandHandlerExecutor _commandHandlerExecutor;

    public CustomersController(ICommandHandlerExecutor commandHandlerExecutor)
    {
        _commandHandlerExecutor = commandHandlerExecutor;
    }

    [HttpPost]
    public async Task<IActionResult> AddCustomer(AddCustomerCommand command, CancellationToken ct)
    {
        await _commandHandlerExecutor.Execute(command, ct);

        return Ok();
    }
}
