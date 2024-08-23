using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Customers.Domain.Carts;
public class Payment : ValueObject
{
    public PaymentId PaymentId { get; }
    public string PaymentType { get; }

    public Payment(PaymentId paymentId, string paymentType)
    {
        PaymentId = paymentId;
        PaymentType = paymentType;
    }
}
