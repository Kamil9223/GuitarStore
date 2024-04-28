using Domain;

namespace Orders.Domain.Orders;

public class OrderItem : Entity
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    private OrderItem(string name, decimal price, int quantity)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public static OrderItem Create(string name, decimal price, int quantity)
    {
        return new OrderItem(name, price, quantity);
    }
}
