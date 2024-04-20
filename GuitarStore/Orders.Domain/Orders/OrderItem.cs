using Domain;

namespace Orders.Domain.Orders;

public class OrderItem : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public uint Quantity { get; }
    public int OrderId { get; }

    private OrderItem(int id, string name, decimal price, uint quantity)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    internal static OrderItem Create(int id, string name, decimal price, uint quantity)
    {
        return new OrderItem(id, name, price, quantity);
    }
}
