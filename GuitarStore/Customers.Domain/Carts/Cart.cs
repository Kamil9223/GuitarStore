using Customers.Domain.Products;
using Domain;

namespace Customers.Domain.Carts;

public class Cart : Entity
{
    public int CustomerId { get; }

    private List<CartItem> _cartItems;
    public IReadOnlyCollection<CartItem> CartItems => _cartItems;

    public decimal TotalPrice => CartItems.Sum(x => x.Price * x.Quantity);

    private Cart(int customerId)
    {
        CustomerId = customerId;
        _cartItems = new List<CartItem>();
    }

    public static Cart Create(int customerId)
    {
        return new Cart(customerId);
    }

    public void ClearCart() => _cartItems.Clear();

    public void AddProduct(Product product, int quantity)
    {
        var existingProduct = CartItems.SingleOrDefault(x => x.ProductId == product.Id);

        if (existingProduct is not null)
        {
            existingProduct.IncreaseQuantity(quantity);
            return;
        }

        _cartItems.Add(CartItem.Create(product.Id, product.Name, product.Price, quantity));
    }

    public void RemoveProduct(int productId, int quantity)
    {
        var existingProduct = CartItems.SingleOrDefault(x => x.ProductId == productId);

        if (existingProduct is null)
        {
            throw new DomainException($"Cannot remove product from cart because product with Id: [{productId}] is not stored in cart.");
        }

        var decreasingQuantityPossible = existingProduct.IsDecreasingQuantityPossible(quantity);

        if (decreasingQuantityPossible)
        {
            existingProduct.DecreaseQuantity(quantity);
            return;
        }

        if (existingProduct.Quantity == quantity)
        {
            _cartItems.Remove(existingProduct);
            return;
        }

        throw new DomainException($"Cannot remove [{quantity}] cartItems with productId = [{existingProduct.ProductId}] because exists only [{existingProduct.Quantity}]");
    }

    public CheckoutCart Checkout()
    {
        if (_cartItems.Count == 0)
        {
            throw new DomainException("Cannot checkout empty Cart.");
        }
        return new(this);
    }
}