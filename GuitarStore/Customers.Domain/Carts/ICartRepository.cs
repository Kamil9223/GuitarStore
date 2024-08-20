using Domain.StronglyTypedIds;

namespace Customers.Domain.Carts;
public interface ICartRepository
{
    Task Add(Cart cart);
    Task Update(Cart cart);
    Task Update(CheckoutCart cart);
    Task<Cart> GetCart(CustomerId customerId);
    Task<CheckoutCart> GetCheckoutCart(CustomerId customerId);
}
