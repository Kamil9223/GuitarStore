using Application.RabbitMq.Abstractions;
using Domain.StronglyTypedIds;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Events.Outgoing;
using Stripe;

namespace GuitarStore.ApiGateway.Modules.Payments;

[Route("webhook")]
[ApiController]
public class WebhookStripeController : ControllerBase
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public WebhookStripeController(IIntegrationEventPublisher integrationEventPublisher)
    {
        _integrationEventPublisher = integrationEventPublisher;
    }

    [HttpPost]
    public async Task<IActionResult> PaymentEvent()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var stripeEvent = EventUtility.ParseEvent(json);

        if (stripeEvent.Type == Events.PaymentIntentSucceeded)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            paymentIntent.Metadata.TryGetValue("OrderID", out string orderId);
            await _integrationEventPublisher.Publish(new OrderPaidEvent((OrderId)orderId));
        }
        else if (stripeEvent.Type == Events.PaymentMethodAttached)
        {
            var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
        }
        else
        {
            Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
        }
        return Ok();
    }
}
