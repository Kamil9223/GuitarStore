using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Orders.Domain.Orders;
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
