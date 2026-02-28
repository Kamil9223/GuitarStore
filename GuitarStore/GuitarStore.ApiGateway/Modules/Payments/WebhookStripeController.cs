using Application.CQRS.Command;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Commands;

namespace GuitarStore.ApiGateway.Modules.Payments;

[Route("webhook")]
[ApiController]
public class WebhookStripeController : ControllerBase
{
    private readonly ICommandHandlerExecutor _commandHandlerExecutor;

    public WebhookStripeController(
        ICommandHandlerExecutor commandHandlerExecutor)
    {
        _commandHandlerExecutor = commandHandlerExecutor;
    }

    [HttpPost]
    public async Task<IActionResult> PaymentEvent(CancellationToken ct)
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync(ct);
        if (!Request.Headers.TryGetValue("Stripe-Signature", out var sigHeader))
            return BadRequest("Missing Stripe-Signature header");

        var sig = sigHeader.ToString();
        await _commandHandlerExecutor.Execute(new StripeWebhookCommand(json, sig), ct);
        return Ok();
    }
}
