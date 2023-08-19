namespace Catalog.Application.Products.Dtos;

public class ProductDetailsDto
{
    public string Brand { get; init; } = null!;
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string Description { get; init; } = null!;
    public string Category { get; init; } = null!;
}
