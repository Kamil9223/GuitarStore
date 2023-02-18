using Customers.Domain.Products;
using Domain;

namespace Customers.Domain.Carts;

public class Cart : Entity, IIdentifiable
{
    public int Id { get; }
    public ICollection<CartItem> CartItems { get; }
    public DateTime CreatedAt { get; }
    public decimal TotalPrice => CartItems.Sum(x => x.Price * x.Quantity);

    private Cart()
    {
        CreatedAt = DateTime.Now;
        CartItems = new List<CartItem>();
    }

    public static Cart Create()
    {
        return new Cart();
    }

    public void ClearCart() => CartItems.Clear();

    public void AddProduct(Product product)
    {
        var existingProduct = CartItems.SingleOrDefault(x => x.Id == product.Id);

        if (existingProduct is not null)
        {
            existingProduct.IncreaseQuantity(product.Quantity);
            return;
        }

        CartItems.Add(CartItem.Create(product.Id, product.Name, product.Price, product.Quantity));
    }

    public void RemoveProduct(int productId, uint quantity)
    {
        var existingProduct = CartItems.SingleOrDefault(x => x.ProductId == productId);

        if (existingProduct is null)
        {
            throw new DomainException($"Cannot remove product from cart because product with Id: [{productId}] is not stored in cart.");
        }

        if (existingProduct.IsQuantityDeacrisingPossible(quantity))
        {
            existingProduct.DecreaseQuantity(quantity);
            return;
        }

        CartItems.Remove(existingProduct);
    }
}