using Domain.StronglyTypedIds;

namespace Payments.Core.Services;

public interface IPaymentIntentParser
{
    bool TryParse(Stripe.Event stripeEvent, out string paymentIntentId, out OrderId orderId);
}

internal sealed class PaymentIntentParser : IPaymentIntentParser
{
    private const string OrderIdKey = "orderId";

    public bool TryParse(Stripe.Event stripeEvent, out string paymentIntentId, out OrderId orderId)
    {
        paymentIntentId = default!;
        orderId = default!;

        if (stripeEvent.Data?.Object is not Stripe.PaymentIntent pi)
            return false;

        if (pi.Id is null)
            return false;

        if (pi.Metadata is null || !pi.Metadata.TryGetValue(OrderIdKey, out var raw))
            return false;

        if (!Guid.TryParse(raw, out var guid))
            return false;

        paymentIntentId = pi.Id;
        orderId = new OrderId(guid);
        return true;
    }
}