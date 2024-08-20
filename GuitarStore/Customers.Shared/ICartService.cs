using Domain.StronglyTypedIds;

namespace Customers.Shared;
public interface ICartService
{
    Task<CheckoutCartDto> GetCheckoutCart(CustomerId customerId);
}
