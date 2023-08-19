using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Services;
using Catalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.QueryServices;

internal class ProductQueryService : IProductQueryService
{
    private readonly CatalogDbContext _catalogDbContext;

    public ProductQueryService(CatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<ProductDetailsDto?> Get(int id)
    {
        return await _catalogDbContext.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailsDto
            {
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Brand = p.Brand.Name,
                Category = p.Category.CategoryName
            })
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public IEnumerable<ProductDto?> Get()
    {
        return _catalogDbContext.Products
            .Select(p => new ProductDto
            {
                Name = p.Name,
                Brand = p.Brand.Name,
                Price = p.Price,
            })
            .AsNoTracking();
    }
}
