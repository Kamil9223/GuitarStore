using Application.Channels;
using Application.CQRS;
using Customers.Shared;
using Domain.StronglyTypedIds;
using Orders.Application.Abstractions;
using Orders.Application.Orders.BackgroundJobs;
using Orders.Domain.Orders;
using Warehouse.Shared;

namespace Orders.Application.Orders.Commands;

public sealed record PlaceOrderCommand(CustomerId CustomerId) : ICommand;

internal sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    private readonly ICartService _cartService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChannelPublisher<OrderCompletionChannelEvent> _orderCompletionChannelPublisher;
    private readonly IProductReservationService _productReservationService;

    public PlaceOrderCommandHandler(
        ICartService cartService,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IChannelPublisher<OrderCompletionChannelEvent> orderCompletionChannelPublisher,
        IProductReservationService productReservationService)
    {
        _cartService = cartService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _orderCompletionChannelPublisher = orderCompletionChannelPublisher;
        _productReservationService = productReservationService;
    }

    public async Task Handle(PlaceOrderCommand command)
    {
        var checkoutCart = await _cartService.GetCheckoutCart(command.CustomerId);

        var newOrder = Order.Create(
            orderItems: OrdersMapper.MapToOrderItems(checkoutCart.Items),
            customerId: checkoutCart.CustomerId,
            deliveryAddress: OrdersMapper.MapToDeliveryAddress(checkoutCart.DeliveryAddress),
            payment: new Payment(checkoutCart.PaymentId, checkoutCart.PaymentType),
            delivery: new Delivery(checkoutCart.DelivererId, checkoutCart.Deliverer));

        await _productReservationService.ReserveProduct(OrdersMapper.MapToReserveProductsDto(newOrder));

        await _orderRepository.Add(newOrder);

        await _unitOfWork.SaveChanges();

        await _orderCompletionChannelPublisher.Publish(new OrderCompletionChannelEvent(newOrder), CancellationToken.None);
    }
}