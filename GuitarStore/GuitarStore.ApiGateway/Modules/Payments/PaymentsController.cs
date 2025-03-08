using Application.CQRS;
using Microsoft.AspNetCore.Mvc;
using Payments.Shared.Contracts;
using Payments.Shared.Services;

namespace GuitarStore.ApiGateway.Modules.Payments;

[ApiController]
[Route("payments")]
public class PaymentsController : ControllerBase
{
    private readonly ICommandHandlerExecutor _commandHandlerExecutor;
    private readonly IQueryHandlerExecutor _queryHandlerExecutor;

    private readonly IStripeService _stripeService;

    public PaymentsController(ICommandHandlerExecutor commandHandlerExecutor, IQueryHandlerExecutor queryHandlerExecutor, IStripeService stripeService)
    {
        _commandHandlerExecutor = commandHandlerExecutor;
        _queryHandlerExecutor = queryHandlerExecutor;
        _stripeService = stripeService;
    }
}
