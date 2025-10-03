using Catalog.Domain;
using Catalog.Domain.IRepositories;
using Catalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Catalog.Infrastructure.Repositories;

internal class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _catalogDbContext;

    public ProductRepository(CatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public void Add(Product product)
    {
        _catalogDbContext.Products.Add(product);
    }

    public async Task<bool> Exists(Expression<Func<Product, bool>> predicate, CancellationToken ct)
    {
        return await _catalogDbContext.Products.AnyAsync(predicate, ct);
    }
}
