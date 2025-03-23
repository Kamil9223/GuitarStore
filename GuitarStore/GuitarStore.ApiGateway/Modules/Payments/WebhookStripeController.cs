using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace GuitarStore.ApiGateway.Modules.Payments;

[Route("webhook")]
[ApiController]
public class WebhookStripeController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PaymentEvent()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);

            // Handle the event
            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                // Then define and call a method to handle the successful payment intent.
                // handlePaymentIntentSucceeded(paymentIntent);
            }
            else if (stripeEvent.Type == Events.PaymentMethodAttached)
            {
                var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                // Then define and call a method to handle the successful attachment of a PaymentMethod.
                // handlePaymentMethodAttached(paymentMethod);
            }
            // ... handle other event types
            else
            {
                // Unexpected event type
                Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
            }
            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest();
        }
    }
}
