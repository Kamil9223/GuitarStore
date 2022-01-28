using Domain;

namespace Warehouse.Domain;

public abstract class GuitarBase : Entity
{
    public string CompanyName { get; private set; }

    public string ModelName { get; private set; }

    public decimal Price { get; private set; }
}

