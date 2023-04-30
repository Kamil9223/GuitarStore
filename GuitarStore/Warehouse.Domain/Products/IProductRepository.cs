using Domain;

namespace Warehouse.Domain.Products;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetProduct(int id);
}
