using Application.Exceptions;
using Customers.Domain.Products;
using Customers.Infrastructure.Database;
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

    public async Task<bool> Exists(Expression<Func<Product, bool>> predicate)
    {
        return await _customersDbContext.Products.AnyAsync(predicate);
    }

    public async Task<Product> Get(int productId)
    {
        var product = await _customersDbContext.Products.SingleOrDefaultAsync(x => x.Id == productId);
        if (product is null)
            throw new NotFoundException($"Product with Id: {productId} not exists.");

        return product;
    }
}
