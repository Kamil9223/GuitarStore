using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Queries;
using Domain.StronglyTypedIds;

namespace Catalog.Application.Products.Services;

public interface IProductQueryService
{
    Task<ProductDetailsDto?> Get(ProductId id, CancellationToken ct);
    Task<List<ProductBasedInfoDto>> GetPaged(
        int limit,
        int offset,
        ListProductsFilter? Filter,
        ListProductsSort? Sort,
        CancellationToken ct);
}
