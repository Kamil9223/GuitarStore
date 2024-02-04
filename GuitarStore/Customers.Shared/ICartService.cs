namespace Customers.Shared;
public interface ICartService
{
    Task<CheckoutCartDto> GetCheckoutCart(int customerId);
}
