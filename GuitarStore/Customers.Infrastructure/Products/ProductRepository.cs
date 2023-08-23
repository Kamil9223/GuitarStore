using Customers.Domain.Products;
using Customers.Infrastructure.Database;

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
}
