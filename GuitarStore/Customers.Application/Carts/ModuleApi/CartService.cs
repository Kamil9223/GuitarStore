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

    public async Task<CheckoutCartDto> GetCheckoutCart(CustomerId customerId, CancellationToken ct)
    {
        var customer = await _customerRepository.Get(customerId, ct);
        var checkoutCart = await _cartRepository.GetCheckoutCart(customerId, ct);

        return CheckoutCartDtoMapper.ToCheckoutCartDto(customer, checkoutCart);
    }
}
