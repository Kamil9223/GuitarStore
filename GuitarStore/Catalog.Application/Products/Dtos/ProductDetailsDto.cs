namespace Catalog.Application.Products.Dtos;

public sealed record ProductDetailsDto(
    string Brand,
    string Name,
    decimal Price,
    string Description,
    string Category);
