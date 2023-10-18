using Domain.ValueObjects;

namespace Customers.Domain.Carts;
public class Payment : ValueObject
{
    public int PaymentId { get; }
    public string PaymentType { get; }
}
