namespace Orders.Domain.Products;
public interface IProductRepository
{
    Task<Product> Get(int id);
}
