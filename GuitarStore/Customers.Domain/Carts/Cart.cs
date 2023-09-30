using Customers.Domain.Products;
using Domain;

namespace Customers.Domain.Carts;

public class Cart : Entity, IIdentifiable
{
    public int Id { get; }
    public int CustomerId { get; }
    public ICollection<CartItem> CartItems { get; }
    public DateTime CreatedAt { get; }

    public decimal TotalPrice => CartItems.Sum(x => x.Price * x.Quantity);

    private Cart(int customerId)
    {
        CustomerId = customerId;
        CreatedAt = DateTime.Now;
        CartItems = new List<CartItem>();
    }

    public static Cart Create(int customerId)
    {
        return new Cart(customerId);
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

    public void RemoveProduct(int productId, int quantity)
    {
        var existingProduct = CartItems.SingleOrDefault(x => x.ProductId == productId);

        if (existingProduct is null)
        {
            throw new DomainException($"Cannot remove product from cart because product with Id: [{productId}] is not stored in cart.");
        }

        var decreasedQuantity = existingProduct.DecreaseQuantity(quantity);

        if (!decreasedQuantity)
            CartItems.Remove(existingProduct);
    }

    public CheckoutCart Checkout()
    {
        if (CartItems.Count == 0)
        {
            throw new DomainException("Cannot checkout empty Cart.");
        }
        return new(this);
    }
}