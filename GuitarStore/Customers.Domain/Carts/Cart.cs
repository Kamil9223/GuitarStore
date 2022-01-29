using Customers.Domain.Products;
using Domain;

namespace Customers.Domain.Carts;

public class Cart : Entity, IIdentifiable
{
    public int Id { get; }
    public ICollection<Product> Products { get; }
    public DateTime CreatedAt { get; }
    public decimal TotalPrice { get => Products.Sum(x => x.Price * x.Quantity); }

    private Cart(int id)
    {
        Id = id;
        CreatedAt = DateTime.Now;
        Products = new List<Product>();
    }

    public static Cart Create(int id)
    {
        return new Cart(id);
    }

    public void ClearCart() => Products.Clear();

    public void AddProduct(Product product)
    {
        var existingProduct = Products.SingleOrDefault(x => x.Id == product.Id);

        if (existingProduct is not null)
        {
            existingProduct.IncreaseQuantity(product.Quantity);
            return;
        }

        Products.Add(product);
    }

    public void RemoveProduct(int productId, uint quantity)
    {
        var existingProduct = Products.SingleOrDefault(x => x.Id == productId);

        if (existingProduct is null)
        {
            throw new Exception(); //TODO domain exception with correct message and code maybe
        }

        if (existingProduct.IsQuantityDeacrisingPossible(quantity))
        {
            existingProduct.DecreaseQuantity(quantity);
            return;
        }

        Products.Remove(existingProduct);
    }
}
