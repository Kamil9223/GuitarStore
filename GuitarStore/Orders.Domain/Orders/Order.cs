using Domain;

namespace Orders.Domain.Orders;

public class Order : Entity
{
    private List<OrderItem> _orderItems;

    public Guid Id { get; }
    public int CustomerId { get; }
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; private set; }
    public DeliveryAddress DeliveryAddress { get; } = null!;
    public Payment Payment { get; } = null!;
    public Delivery Delivery { get; } = null!;


    public decimal TotalPrice { get => _orderItems.Sum(x => x.Price * x.Quantity); }

    private Order(ICollection<OrderItem> orderItems, int customerId, DeliveryAddress deliveryAddress, Payment payment, Delivery delivery)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
        Status = OrderStatus.New;
        _orderItems = orderItems.ToList();
        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        Payment = payment;
        Delivery = delivery;
    }

    public static Order Create(ICollection<OrderItem> orderItems, int customerId, DeliveryAddress deliveryAddress, Payment payment, Delivery delivery)
    {
        return new Order(orderItems, customerId, deliveryAddress, payment, delivery);
    }

    public void AcceptOrder()
    {
        Status = OrderStatus.Accepted;
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

public enum OrderStatus
{
    New = 1,

    Accepted = 2,

    Waiting = 3,

    InProgress = 4,

    Realized = 5,

    Canceled = 6,
}
