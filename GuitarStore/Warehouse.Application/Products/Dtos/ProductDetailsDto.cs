namespace Warehouse.Application.Products.Dtos;

public class ProductDetailsDto
{
    public string? Brand { get; init; }
    public string? Name { get; init; }
    public decimal Price { get; init; }
    public string? Description { get; init; }
    public string? CategoryName { get; init; }
}
