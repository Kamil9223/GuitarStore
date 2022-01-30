using Domain;

namespace Orders.Domain.Products;

public class Product : Entity, IIdentifiable
{
    public int Id { get; }
    public ProductType ProductType { get; }
    public string Name { get; }
    public decimal Price { get; }
    public uint Quantity { get; private set; }

    private Product(int id, ProductType productType, string name, decimal price, uint quantity)
    {
        Id = id;
        ProductType = productType;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    internal static Product Create(int id, ProductType productType, string name, decimal price, uint quantity)
    {
        //Check rules

        return new Product(id, productType, name, price, quantity);
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
