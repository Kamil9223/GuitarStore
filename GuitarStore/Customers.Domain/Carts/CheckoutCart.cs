using Domain;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Customers.Domain.Carts;

public class CheckoutCart : Entity
{
    public CustomerId CustomerId { get; }
    public ICollection<CartItem> CartItems { get; }
    public Delivery Delivery { get; private set; }

    internal CheckoutCart(Cart cart)
    {
        CustomerId = cart.CustomerId;
        CartItems = cart.CartItems.ToList();
    }

    public void SetModelOfDelivery(Delivery delivery) => Delivery = delivery;
}
