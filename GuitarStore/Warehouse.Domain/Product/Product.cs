using Domain;
using Domain.ValueObjects;
using Warehouse.Domain.Store;

namespace Warehouse.Domain.Product;

public class Product : Entity, IIdentifiable
{
    public int Id { get; }
    public int CategoryId { get; }
    public string ProducerName { get; }
    public string Name { get; }
    public Money Price { get; }
    public string Description { get; }
    public int GuitarStoreId { get; }
    public Category Category { get; }
    public GuitarStore GuitarStore { get; }
}
