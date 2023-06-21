using Domain;

namespace Catalog.Domain.Products;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetProduct(int id);
}
