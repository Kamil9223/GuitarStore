using Domain.StronglyTypedIds;

namespace Catalog.Application.Products.Dtos;
public sealed record ProductBasedInfoDto(ProductId Id, string Name, decimal Price, int Quantity);
