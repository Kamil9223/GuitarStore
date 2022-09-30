using Domain;
using Warehouse.Domain.Store;

namespace Warehouse.Domain.Product;

public class Product : Entity, IIdentifiable
{
    public int Id { get; }
    public int CategoryId { get; }
    public Category Category { get; }
    public string ProducerName { get; }
    public string ModelName { get; }
    public decimal Price { get; }
    public string Description { get; }
    public int GuitarStoreId { get; }
    public GuitarStore GuitarStore { get; }

    //For EF Core
    private Product() { }

    private Product(int categoryId, string producerName, string modelName, decimal price, string description, int guitarStoreId)
    {
        CategoryId = categoryId;
        ProducerName = producerName;
        ModelName = modelName;
        Price = price;
        Description = description;
        GuitarStoreId = guitarStoreId;
    }

    internal static Product Create(int categoryId, string producerName, string modelName, decimal price, string description, int guitarStoreId)
    {
        //Check rules?

        return new Product(categoryId, producerName, modelName, price, description, guitarStoreId);
    }
}
