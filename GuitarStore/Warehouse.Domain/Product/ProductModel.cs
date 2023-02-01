using Domain.ValueObjects;

namespace Warehouse.Domain.Product;

public class ProductModel : ValueObject
{
    public string ProducerName { get; }
    public string Name { get; }

    //For EF Core
    private ProductModel() { }

    private ProductModel(string producerName, string name)
    {
        ProducerName = producerName;
        Name = name;
    }

    public static ProductModel Create(string producerName, string name)
    {
        return new ProductModel(producerName, name);
    }
}
