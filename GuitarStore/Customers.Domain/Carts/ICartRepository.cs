namespace Customers.Domain.Carts;
public interface ICartRepository
{
    Task Add(Cart cart);
}
