using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Customers.Shared;
using Domain.StronglyTypedIds;

namespace Customers.Application.Carts.ModuleApi;
internal class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICustomerRepository _customerRepository;

    public CartService(ICartRepository cartRepository, ICustomerRepository customerRepository)
    {
        _cartRepository = cartRepository;
        _customerRepository = customerRepository;
    }

    public async Task<CheckoutCartDto> GetCheckoutCart(CustomerId customerId)
    {
        var customer = await _customerRepository.Get(customerId);
        var checkoutCart = await _cartRepository.GetCheckoutCart(customerId);

        return CheckoutCartDtoMapper.ToCheckoutCartDto(customer, checkoutCart);
    }
}
