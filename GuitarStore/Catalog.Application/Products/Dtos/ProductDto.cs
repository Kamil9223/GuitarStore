namespace Catalog.Application.Products.Dtos;

public class ProductDto
{
    public string Brand { get; init; } = null!;
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
}
