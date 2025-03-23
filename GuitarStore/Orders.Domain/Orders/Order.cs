using Domain;
using Domain.StronglyTypedIds;

namespace Orders.Domain.Orders;

public class Order : Entity
{
    private List<OrderItem> _orderItems;

    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; private set; }
    public DeliveryAddress DeliveryAddress { get; } = null!;
    public Delivery Delivery { get; } = null!;


    public decimal TotalPrice { get => _orderItems.Sum(x => x.Price * x.Quantity); }

    private Order(ICollection<OrderItem> orderItems, CustomerId customerId, DeliveryAddress deliveryAddress, Delivery delivery)
    {
        Id = new OrderId(Guid.NewGuid());
        CreatedAt = DateTime.Now;
        Status = OrderStatus.New;
        _orderItems = orderItems.ToList();
        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        Delivery = delivery;
    }

    public static Order Create(ICollection<OrderItem> orderItems, CustomerId customerId, DeliveryAddress deliveryAddress, Delivery delivery)
    {
        return new Order(orderItems, customerId, deliveryAddress, delivery);
    }

    public void AcceptOrder()
    {
        Status = OrderStatus.Paid;
    }

    public void CancelOrder()
    {
        if (Status == OrderStatus.Realized)
        {
            throw new DomainException("Cannot cancel already relized order.");
        }

        Status = OrderStatus.Canceled;
    }
}

public enum OrderStatus : byte
{
    New = 1,

    Paid = 2,

    Sent = 3,

    Realized = 4,

    Canceled = 5,
}
