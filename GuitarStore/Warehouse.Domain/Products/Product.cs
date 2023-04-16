using Domain;
using Domain.ValueObjects;
using Warehouse.Domain.Categories;

namespace Warehouse.Domain.Products;

public class Product : Entity, IIdentifiable
{
    public int Id { get; }
    public string? Brand { get; }
    public string? Name { get; }
    public Money? Price { get; }
    public string? Description { get; }
    public Category? Category { get; }
}
