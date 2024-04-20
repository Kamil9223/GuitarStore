using Domain.ValueObjects;

namespace Orders.Domain.Orders;
public class Payment : ValueObject
{
    public int PaymentId { get; }
    public string PaymentType { get; }

    public Payment(int paymentId, string paymentType)
    {
        PaymentId = paymentId;
        PaymentType = paymentType;
    }
}
