using Common.Errors.Exceptions;
using Domain;
using Domain.StronglyTypedIds;
using Newtonsoft.Json;

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

    //For deserialization
    [JsonConstructor]
    private Order(
        OrderId id,
        ICollection<OrderItem> orderItems,
        CustomerId customerId,
        DeliveryAddress deliveryAddress,
        Delivery delivery)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.PendingPayment;
        _orderItems = orderItems.ToList();
        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        Delivery = delivery;
    }

    public static Order Create(ICollection<OrderItem> orderItems, CustomerId customerId, DeliveryAddress deliveryAddress, Delivery delivery)
    {
        return new Order(OrderId.New(), orderItems, customerId, deliveryAddress, delivery);
    }

    public void MarkPaid()
    {
        if (Status == OrderStatus.Paid)
            return;
        
        if (Status != OrderStatus.PendingPayment)
            throw new DomainException($"Cannot mark paid from status {Status}.");

        Status = OrderStatus.Paid;
    }

    public void MarkSent()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainException($"Cannot mark sent from status {Status}.");

        Status = OrderStatus.Sent;
    }

    public void MarkRealized()
    {
        if (Status != OrderStatus.Sent)
            throw new DomainException($"Cannot mark realized from status {Status}.");

        Status = OrderStatus.Realized;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Paid or OrderStatus.Sent or OrderStatus.Realized)
            throw new DomainException($"Cannot cancel order in status {Status}.");

        if (Status is OrderStatus.Canceled or OrderStatus.Expired)
            return;

        Status = OrderStatus.Canceled;
    }

    public void Expire()
    {
        if (Status == OrderStatus.Expired)
            return; // idempotent

        if (Status != OrderStatus.PendingPayment)
            throw new DomainException($"Cannot expire order in status {Status}.");

        Status = OrderStatus.Expired;
    }
}

public enum OrderStatus : byte
{
    /// <summary>
    /// The order has been created in the system.
    /// Products have been successfully reserved (soft reservation with TTL),
    /// and the system is waiting for payment confirmation (e.g., Stripe webhook).
    /// The order may transition to Paid, Canceled, or Expired.
    /// </summary>
    PendingPayment = 1,

    /// <summary>
    /// Payment has been successfully confirmed.
    /// The reservation should be confirmed in the Warehouse module.
    /// The order is ready for fulfillment.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// The order has been dispatched for delivery.
    /// Fulfillment has started or shipment has been sent to the customer.
    /// </summary>
    Sent = 3,

    /// <summary>
    /// The order has been fully completed and delivered.
    /// No further state transitions are expected in the standard flow.
    /// </summary>
    Realized = 4,

    /// <summary>
    /// The order has been canceled.
    /// This may happen due to payment failure, manual cancellation,
    /// or business rule violation (before fulfillment).
    /// </summary>
    Canceled = 5,

    /// <summary>
    /// The order has expired because the payment was not completed
    /// within the allowed reservation time (TTL).
    /// Reserved stock should be released.
    /// </summary>
    Expired = 6
}
