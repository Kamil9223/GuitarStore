using Domain;

namespace Orders.Domain.Orders;

public class Order : Entity, IIdentifiable
{
    private List<OrderItem> _orderItems;

    public int Id { get; }
    public int CustomerId { get; }  
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; }



    public decimal TotalPrice { get => _orderItems.Sum(x => x.Price * x.Quantity); }
    

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
