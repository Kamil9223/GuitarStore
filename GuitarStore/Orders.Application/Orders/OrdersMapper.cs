using Customers.Shared;
using Orders.Domain.Orders;
using Warehouse.Shared;

namespace Orders.Application.Orders;
public static class OrdersMapper
{
    public static DeliveryAddress MapToDeliveryAddress(CheckoutCartDto.Address checkoutCartAddress)
       => new(
               country: checkoutCartAddress.Country,
               localityName: checkoutCartAddress.LocalityName,
               postalCode: checkoutCartAddress.PostalCode,
               houseNumber: checkoutCartAddress.HouseNumber,
               street: checkoutCartAddress.Street,
               localNumber: checkoutCartAddress.LocalNumber
           );

    public static ICollection<OrderItem> MapToOrderItems(IReadOnlyCollection<CheckoutCartDto.CheckoutCartItem> checkoutCartItems)
        => checkoutCartItems.Select(x => OrderItem.Create(
                name: x.Name,
                price: x.Amount,
                quantity: x.Quantity,
                productId: x.ProductId
            )).ToList();

    public static ReserveProductsDto MapToReserveProductsDto(Order order)
        => new(
                order.Id,
                order.OrderItems.Select(x => new ReserveProductDto(
                    x.ProductId,
                    x.Quantity))
                .ToList());
}
