using Domain;
using Domain.ValueObjects;

namespace Customers.Domain.Products;

public class Product : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public Money Price { get; private set; }
    public int Quantity { get; private set; }

    //For EF Core
    private Product() { }

    private Product(int id, string name, Money price, int quantity)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public static Product Create(int id, string name, Money price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Provided product name: [{name}] is not valid.");
        }

        if (quantity <= 0)
        {
            throw new DomainException($"Product quantity must be greater than zero.");
        }

        return new Product(id, name, price, quantity);
    }

    public void IncreaseQuantity(int quantity) => Quantity += quantity;

    internal void DecreaseQuantity(int quantity)
    {
        if (quantity > Quantity)
        {
            throw new DomainException($"Cannot decrease quantity of product.");
        }

        Quantity -= quantity;
    }

    internal bool IsQuantityDeacrisingPossible(uint quantity) => Quantity > quantity;
}
