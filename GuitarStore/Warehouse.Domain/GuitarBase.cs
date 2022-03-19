using Domain;

namespace Warehouse.Domain;

public abstract class GuitarBase : Entity
{
    public string CompanyName { get; }

    public string ModelName { get; }

    public decimal Price { get; }

    //For EF Core
    protected GuitarBase() { }

    protected GuitarBase(string companyName, string modelName, decimal price)
    {
        CompanyName = companyName;
        ModelName = modelName;
        Price = price;
    }
}

