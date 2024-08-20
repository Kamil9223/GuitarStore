using Domain.StronglyTypedIds;
using System.Linq.Expressions;

namespace Customers.Domain.Products;

public interface IProductRepository
{
    void Add(Product product);
    Task<bool> Exists(Expression<Func<Product, bool>> predicate);
    Task<Product> Get(ProductId productId);
    Task<Product> Get(string name);
}
