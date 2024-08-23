using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Customers.Domain.Carts;
public class Delivery : ValueObject
{
    public DelivererId DelivererId { get; }
    public string Deliverer { get; }

    public Delivery(DelivererId delivererId, string deliverer)
    {
        DelivererId = delivererId;
        Deliverer = deliverer;
    }
}
