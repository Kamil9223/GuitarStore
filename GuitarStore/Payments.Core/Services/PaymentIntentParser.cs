using Domain.StronglyTypedIds;

namespace Payments.Core.Services;

public interface IPaymentIntentParser
{
    (string PaymentIntentId, OrderId OrderId) ParseOrThrow(Stripe.Event stripeEvent);
}

internal sealed class PaymentIntentParser : IPaymentIntentParser
{
    private const string OrderIdKey = "orderId";

    public (string PaymentIntentId, OrderId OrderId) ParseOrThrow(Stripe.Event stripeEvent)
    {
        if (stripeEvent.Data?.Object is not Stripe.PaymentIntent pi)
            throw new InvalidOperationException($"Expected PaymentIntent in event data, got: {stripeEvent.Data?.Object?.GetType().Name ?? "null"}");

        var piId = pi.Id ?? throw new InvalidOperationException("PaymentIntent.Id missing");

        if (pi.Metadata is null || !pi.Metadata.TryGetValue(OrderIdKey, out var orderIdRaw))
            throw new InvalidOperationException($"PaymentIntent.Metadata missing '{OrderIdKey}'");

        if (!Guid.TryParse(orderIdRaw, out var orderId))
            throw new InvalidOperationException($"Invalid '{OrderIdKey}' metadata value: {orderIdRaw}");

        return (piId, new OrderId(orderId));
    }
}