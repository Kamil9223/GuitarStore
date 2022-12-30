using Domain;

namespace Warehouse.Domain.Store;

public class GuitarStore : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; private set; }
    public StoreLocation Location { get; private set; }
    public ICollection<Product.Product> Products { get; }

    //For EF Core
    private GuitarStore() { }

    private GuitarStore(string name, StoreLocation location)
    {
        Name = name;
        Location = location;
        Products = new List<Product.Product>();
    }

    public static GuitarStore Create(string name, StoreLocation storeLocation)
    {
        return new GuitarStore(name, storeLocation);
    }

    public void AddProduct(int categoryId, string producerName, string modelName, decimal price, string description, int guitarStoreId)
    {
        Products.Add(Product.Product.Create(categoryId, producerName, modelName, price, description, guitarStoreId));
    }
}
