using System.Linq.Expressions;

namespace Catalog.Domain.IRepositories;

public interface IProductRepository
{
    void Add(Product product);
    Task<bool> Exists(Expression<Func<Product, bool>> predicate);
}
