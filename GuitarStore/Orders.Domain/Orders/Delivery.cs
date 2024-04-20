using Domain.ValueObjects;

namespace Orders.Domain.Orders;
public class Delivery : ValueObject
{
    public int DelivererId { get; }
    public string Deliverer { get; }

    public Delivery(int delivererId, string deliverer)
    {
        DelivererId = delivererId;
        Deliverer = deliverer;
    }
}
