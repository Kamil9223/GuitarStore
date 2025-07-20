using Domain;
using Domain.StronglyTypedIds;
using Newtonsoft.Json;

namespace Orders.Domain.Orders;

public class OrderItem : Entity
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public ProductId ProductId { get; }

    //For deserialization
    [JsonConstructor]
    private OrderItem(Guid id, string name, decimal price, int quantity, ProductId productId)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
        ProductId = productId;
    }

    public static OrderItem Create(string name, decimal price, int quantity, ProductId productId)
    {
        return new OrderItem(Guid.NewGuid(), name, price, quantity, productId);
    }
}
