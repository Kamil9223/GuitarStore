using Domain;
using Domain.ValueObjects;
using Warehouse.Domain.Categories;

namespace Warehouse.Domain.Products;

public class Product : Entity, IIdentifiable
{
    public int Id { get; init; }
    public string? Brand { get; init; }
    public string? Name { get; init; }
    public Money? Price { get => _price; init => _price = value; }
    public string? Description { get => _description; init => _description = value; }
    public Category? Category { get; init; }

    private string? _description;
    private Money? _price;

    public void UpdateProduct(string? description, Money? price)
    {
        _description = description;
        _price = price;
    }
}
