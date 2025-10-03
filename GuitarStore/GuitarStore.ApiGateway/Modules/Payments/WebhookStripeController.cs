using Common.RabbitMq.Abstractions.EventHandlers;
using Domain.StronglyTypedIds;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Events.Outgoing;
using Payments.Shared.Services;
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
    public async Task<IActionResult> PaymentEvent(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var stripeEvent = EventUtility.ParseEvent(json);

        if (stripeEvent.Type == Events.PaymentIntentSucceeded)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            var orderId = paymentIntent!.Metadata.GetValueOrDefault(IStripeService.OrderIdMetadataKey);
            if (string.IsNullOrEmpty(orderId))
            {
                //log error, critical or smthg. It should not happen
            }
            var typedOrderId = (OrderId)orderId!;
            await _integrationEventPublisher.Publish(new OrderPaidEvent(typedOrderId), ct); //orderId jest otrzymywany, ale event nie dochodzi do handlera, zbadać
        }
        else if (stripeEvent.Type == Events.PaymentIntentCanceled)
        {
            var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
            //handle cancellation
        }
        else
        {
            Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
        }
        return Ok();
    }
}
