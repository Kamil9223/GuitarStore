using Domain.StronglyTypedIds;

namespace Orders.Domain.Products;
public interface IProductRepository
{
    Task<Product> Get(ProductId id);
}
