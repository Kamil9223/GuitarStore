using Domain;
using Domain.ValueObjects;
using Warehouse.Domain.Categories;

namespace Warehouse.Domain.Products;

public class Product : Entity, IIdentifiable
{
    public int Id { get; init; }
    public string? Brand { get; init; }
    public string? Name { get; init; }
    public Money? Price { get; init; }
    public string? Description { get; init; }
    public Category? Category { get; init; }
}
