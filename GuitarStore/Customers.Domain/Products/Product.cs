using Domain;

namespace Customers.Domain.Products;

public class Product : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public uint Quantity { get; private set; }

    private Product(int id, string name, decimal price, uint quantity)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    internal static Product Create(int id, string name, decimal price, uint quantity)
    {
        //Check rules

        return new Product(id, name, price, quantity);
    }

    internal void IncreaseQuantity(uint quantity) => Quantity += quantity;

    internal void DecreaseQuantity(uint quantity)
    {
        if (quantity >= Quantity)
        {
            throw new Exception(); //TODO: domain exception with message and code maybe
        }

        Quantity -= quantity;
    }

    internal bool IsQuantityDeacrisingPossible(uint quantity) => Quantity > quantity;
}
