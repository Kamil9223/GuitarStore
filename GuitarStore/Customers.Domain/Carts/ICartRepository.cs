using Domain.StronglyTypedIds;

namespace Customers.Domain.Carts;
public interface ICartRepository
{
    Task Add(Cart cart, CancellationToken ct);
    Task Update(Cart cart, CancellationToken ct);
    Task Update(CheckoutCart cart, CancellationToken ct);
    Task<Cart> GetCart(CustomerId customerId, CancellationToken ct);
    Task<CheckoutCart> GetCheckoutCart(CustomerId customerId, CancellationToken ct);
}
