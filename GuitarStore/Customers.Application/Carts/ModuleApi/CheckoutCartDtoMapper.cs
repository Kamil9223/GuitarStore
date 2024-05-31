using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Customers.Shared;

namespace Customers.Application.Carts.ModuleApi;
internal static class CheckoutCartDtoMapper
{
    public static CheckoutCartDto ToCheckoutCartDto(Customer customer, CheckoutCart checkout)
        => new()
        {
            CustomerId = customer.Id,
            DeliveryAddress = new CheckoutCartDto.Address
            {
                Country = customer.Address.Country,
                HouseNumber = customer.Address.HouseNumber,
                LocalityName = customer.Address.LocalityName,
                LocalNumber = customer.Address.LocalNumber,
                PostalCode = customer.Address.PostalCode,
                Street = customer.Address.Street
            },
            Deliverer = checkout.Delivery.Deliverer,
            DelivererId = checkout.Delivery.DelivererId,
            PaymentType = checkout.Payment.PaymentType,
            PaymentId = checkout.Payment.PaymentId,
            Items = checkout.CartItems.Select(item => new CheckoutCartDto.CheckoutCartItem
            {
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                ProductId = item.ProductId,
            }).ToList()
        };
}
