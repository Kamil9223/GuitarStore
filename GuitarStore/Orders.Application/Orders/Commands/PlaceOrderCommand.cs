using Application.CQRS;
using Application.RabbitMq.Abstractions;
using Customers.Shared;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Orders.Application.Abstractions;
using Orders.Application.Orders.Events.Outgoing;
using Orders.Domain.Orders;
using Warehouse.Shared;

namespace Orders.Application.Orders.Commands;

public sealed record PlaceOrderCommand(CustomerId CustomerId) : ICommand;

internal sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    private readonly ICartService _cartService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductReservationService _productReservationService;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public PlaceOrderCommandHandler(
        ICartService cartService,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IProductReservationService productReservationService,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _cartService = cartService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _productReservationService = productReservationService;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(PlaceOrderCommand command)
    {
        var checkoutCart = await _cartService.GetCheckoutCart(command.CustomerId);

        var newOrder = Order.Create(
            orderItems: OrdersMapper.MapToOrderItems(checkoutCart.Items),
            customerId: checkoutCart.CustomerId,
            deliveryAddress: OrdersMapper.MapToDeliveryAddress(checkoutCart.DeliveryAddress),
            payment: checkoutCart.PaymentMethod,
            delivery: new Delivery(checkoutCart.DelivererId, checkoutCart.Deliverer));

        await _productReservationService.ReserveProduct(OrdersMapper.MapToReserveProductsDto(newOrder));

        await _orderRepository.Add(newOrder);

        await _integrationEventPublisher.Publish(new CreatedOrderEvent(
            OrderId: newOrder.Id,
            TotalAmount: newOrder.TotalPrice,
            Currency: Currency.PLN,
            PaymentMethod: checkoutCart.PaymentMethod));

        await _unitOfWork.SaveChanges();
    }

    private static Payments.Shared.Contracts.PaymentMethod MapPaymentMethod(PaymentMethod pm)
    {
        return pm switch
        {
            PaymentMethod.Card => Payments.Shared.Contracts.PaymentMethod.Card,
            PaymentMethod.Blik => Payments.Shared.Contracts.PaymentMethod.Blik,
            PaymentMethod.Link => Payments.Shared.Contracts.PaymentMethod.Link,
            _ => throw new NotImplementedException()
        };
    }
}