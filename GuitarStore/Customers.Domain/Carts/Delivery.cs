using Domain.ValueObjects;

namespace Customers.Domain.Carts;
public class Delivery : ValueObject
{
    public int DelivererId { get; }
    public string Deliverer { get; }
}
