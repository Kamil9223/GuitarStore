using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Products;

namespace Warehouse.Infrastructure.Products;

internal class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(DbContext context) : base(context)
    {
    }

    public async Task<Product?> GetProduct(int id)
    {
        return await _context.Set<Product>()
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == id);
    }
}
