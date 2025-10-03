using Customers.Domain.Products;
using Customers.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Customers.Infrastructure.Products;

internal class ProductRepository : IProductRepository
{
    private readonly CustomersDbContext _customersDbContext;

    public ProductRepository(CustomersDbContext customersDbContext)
    {
        _customersDbContext = customersDbContext;
    }

    public void Add(Product product)
    {
        _customersDbContext.Products.Add(product);
    }

    public async Task<bool> Exists(Expression<Func<Product, bool>> predicate, CancellationToken ct)
    {
        return await _customersDbContext.Products.AnyAsync(predicate, ct);
    }

    public async Task<Product> Get(ProductId productId, CancellationToken ct)
    {
        return await _customersDbContext.Products.SingleOrDefaultAsync(x => x.Id == productId, ct);
    }

    public async Task<Product> Get(string name, CancellationToken ct)
    {
        return await _customersDbContext.Products.SingleOrDefaultAsync(x => x.Name == name, ct);
    }
}
