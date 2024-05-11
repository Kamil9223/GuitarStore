using Application.Channels;
using Application.CQRS;
using Customers.Shared;
using Orders.Application.Abstractions;
using Orders.Application.Orders.BackgroundJobs;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.Commands;

public sealed record PlaceOrderCommand(int CustomerId) : ICommand;

internal sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    private readonly ICartService _cartService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChannelPublisher<OrderCompletionChannelEvent> _orderCompletionChannelPublisher;

    public PlaceOrderCommandHandler(ICartService cartService, IOrderRepository orderRepository, IUnitOfWork unitOfWork, IChannelPublisher<OrderCompletionChannelEvent> orderCompletionChannelPublisher)
    {
        _cartService = cartService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _orderCompletionChannelPublisher = orderCompletionChannelPublisher;
    }

    public async Task Handle(PlaceOrderCommand command)
    {
        var checkoutCart = await _cartService.GetCheckoutCart(command.CustomerId);

        var newOrder = Order.Create(
            orderItems: MapToOrderItems(checkoutCart.Items),
            customerId: checkoutCart.CustomerId,
            deliveryAddress: MapToDeliveryAddress(checkoutCart.DeliveryAddress),
            payment: new Payment(checkoutCart.PaymentId, checkoutCart.PaymentType),
            delivery: new Delivery(checkoutCart.DelivererId, checkoutCart.Deliverer));

        await _orderRepository.Add(newOrder);

        await _orderCompletionChannelPublisher.Publish(new OrderCompletionChannelEvent(newOrder), CancellationToken.None);

        await _unitOfWork.SaveChanges();
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