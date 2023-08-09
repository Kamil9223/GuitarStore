using Catalog.Domain;
using Catalog.Domain.IRepositories;
using Catalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

internal class BrandRepository : IBrandRepository
{
    private readonly CatalogDbContext _catalogDbContext;

    public BrandRepository(CatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<Brand> Get(int id)
    {
        return await _catalogDbContext.Brands.SingleOrDefaultAsync(b => b.Id == id);
    }
}
