using Domain;
using Domain.StronglyTypedIds;
using Newtonsoft.Json;

namespace Customers.Domain.Carts;

public class CheckoutCart : Entity
{
    public CustomerId CustomerId { get; }
    public ICollection<CartItem> CartItems { get; }
    public Delivery Delivery { get; private set; }

    //For deserialization
    [JsonConstructor]
    private CheckoutCart(CustomerId customerId, ICollection<CartItem> cartItems, Delivery delivery)
    {
        CustomerId = customerId;
        CartItems = cartItems;
        Delivery = delivery;
    }

    internal CheckoutCart(Cart cart)
    {
        CustomerId = cart.CustomerId;
        CartItems = cart.CartItems.ToList();
    }

    public void SetModelOfDelivery(Delivery delivery) => Delivery = delivery;
}
