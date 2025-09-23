using Application.Extensions;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Queries;
using Catalog.Application.Products.Services;
using Catalog.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.QueryServices;

internal class ProductQueryService : IProductQueryService
{
    private readonly CatalogDbContext _catalogDbContext;

    public ProductQueryService(CatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<ProductDetailsDto?> Get(ProductId id, CancellationToken ct)
    {
        return await _catalogDbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailsDto(
                p.Brand.Name,
                p.Name,
                p.Price,
                p.Description,
                p.Category.CategoryName))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<ProductBasedInfoDto>> GetPaged(
        int limit,
        int offset,
        ListProductsFilter? Filter,
        ListProductsSort? Sort,
        CancellationToken ct)
    {
        var query = _catalogDbContext.Products
            .AsNoTracking()
            .Select(p => new ProductBasedInfoDto(p.Id, p.Name, p.Price, p.Quantity));

        if (Filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(Filter.Name))
                query = query.Where(x => EF.Functions.Like(x.Name, Filter.Name + "%"));
            if (Filter.MinimumQuantity is not null)
                query = query.Where(x => x.Quantity >= Filter.MinimumQuantity);
        }     

        return await SortQuery()
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);


        IOrderedQueryable<ProductBasedInfoDto> SortQuery()
        {
            if (Sort is not null)
            {
                if (Sort.Name is not null)
                    return query.SortBy(x => x.Name, Sort.Name.Value);
                if (Sort.Price is not null)
                    return query.SortBy(x => x.Price, Sort.Price.Value);
                if (Sort?.Quantity is not null)
                    return query.SortBy(x => x.Quantity, Sort.Quantity.Value);
            }
            return query.OrderBy(x => x.Id);
        }
    }
}
