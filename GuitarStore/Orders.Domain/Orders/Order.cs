using Domain;
using Orders.Domain.OrderItems;

namespace Orders.Domain.Orders;

public class Order : Entity, IIdentifiable
{
    public int Id { get; }
    public ICollection<OrderItem> OrderItems { get; }
    public DateTime CreatedAt { get; }
    public decimal TotalPrice { get => OrderItems.Sum(x => x.Price * x.Quantity); }
    public OrderStatus Status { get; }

    private Order(ICollection<OrderItem> orderItems)
    {
        CreatedAt = DateTime.Now;
        OrderItems = orderItems;
        Status = OrderStatus.New;
    }

    public static Order Create(ICollection<OrderItem> orderItems)
    {
        //Check rules


        return new Order(orderItems);
    }


}
