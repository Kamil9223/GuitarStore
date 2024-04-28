using Application.CQRS;
using Customers.Shared;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.Commands;

public sealed record PlaceOrderCommand(int CustomerId) : ICommand;

internal sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    private readonly ICartService _cartService;

    public PlaceOrderCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task Handle(PlaceOrderCommand command)
    {
        var checkoutCart = await _cartService.GetCheckoutCart(command.CustomerId);

        Order.Create(
            orderItems: MapToOrderItems(checkoutCart.Items),
            customerId: checkoutCart.CustomerId,
            deliveryAddress: MapToDeliveryAddress(checkoutCart.DeliveryAddress),
            payment: new Payment(checkoutCart.PaymentId, checkoutCart.PaymentType),
            delivery: new Delivery(checkoutCart.DelivererId, checkoutCart.Deliverer));

        //Add to DB and Save changes

        //emit channel event for starting order completion
    }

    private DeliveryAddress MapToDeliveryAddress(CheckoutCartDto.Address checkoutCartAddress)
        => new(
                country: checkoutCartAddress.Country,
                localityName: checkoutCartAddress.LocalityName,
                postalCode: checkoutCartAddress.PostalCode,
                houseNumber: checkoutCartAddress.HouseNumber,
                street: checkoutCartAddress.Street,
                localNumber: checkoutCartAddress.LocalNumber
            );

    private ICollection<OrderItem> MapToOrderItems(IReadOnlyCollection<CheckoutCartDto.CheckoutCartItem> checkoutCartItems)
        => checkoutCartItems.Select(x => OrderItem.Create(
                name: x.Name,
                price: x.Price,
                quantity: x.Quantity
            )).ToList();
}
