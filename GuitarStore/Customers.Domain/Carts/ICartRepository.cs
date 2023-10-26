namespace Customers.Domain.Carts;
public interface ICartRepository
{
    Task Add(Cart cart);
    Task Update(Cart cart);
    Task<Cart> GetCart(int customerId);
}
