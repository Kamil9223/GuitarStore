using Domain;
using Domain.ValueObjects;
using Warehouse.Domain.Product;

namespace Warehouse.Domain.Store;

public class GuitarStore : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; private set; }
    public StoreLocation Location { get; private set; }
    public ICollection<Product.Product> Products { get; }

    //For EF Core
    private GuitarStore() { }

    private GuitarStore(string name, StoreLocation location, ICollection<Product.Product> products)
    {
        Name = name;
        Location = location;
        Products = products;
    }

    public static GuitarStore Create(string name, StoreLocation storeLocation)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Provided property [Name]: [{name}] is invalid.");
        }

        return new GuitarStore(name, storeLocation, new List<Product.Product>());
    }

    public void AddProduct(int categoryId, ProductModel productModel, Money price, string description, int guitarStoreId)
    {
        Products.Add(Product.Product.Create(categoryId, productModel, price, description, guitarStoreId));
    }

    public void ChangeLocation(StoreLocation storeLocation)
    {
        Location = storeLocation;
    }
}
