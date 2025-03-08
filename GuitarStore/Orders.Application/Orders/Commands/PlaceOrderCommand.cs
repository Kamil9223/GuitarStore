﻿using Application.CQRS;
using Application.RabbitMq.Abstractions;
using Customers.Shared;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Orders.Application.Abstractions;
using Orders.Application.Orders.Events.Outgoing;
using Orders.Domain.Orders;
using Payments.Shared.Contracts;
using Payments.Shared.Services;
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
    private readonly IStripeService _stripeService;

    public PlaceOrderCommandHandler(
        ICartService cartService,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IProductReservationService productReservationService,
        IIntegrationEventPublisher integrationEventPublisher,
        IStripeService stripeService)
    {
        _cartService = cartService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _productReservationService = productReservationService;
        _integrationEventPublisher = integrationEventPublisher;
        _stripeService = stripeService;
    }

    public async Task Handle(PlaceOrderCommand command)
    {
        var checkoutCart = await _cartService.GetCheckoutCart(command.CustomerId);

        var newOrder = Order.Create(
            orderItems: OrdersMapper.MapToOrderItems(checkoutCart.Items),
            customerId: checkoutCart.CustomerId,
            deliveryAddress: OrdersMapper.MapToDeliveryAddress(checkoutCart.DeliveryAddress),
            delivery: new Delivery(checkoutCart.DelivererId, checkoutCart.Deliverer));

        await _productReservationService.ReserveProduct(OrdersMapper.MapToReserveProductsDto(newOrder));//TODO: obsługa błędu

        var checkoutSession = new CheckoutSessionRequest
        {
            Products = checkoutCart.Items.Select(x => new CheckoutSessionRequest.ProductItem
            {
                Currency = Currency.PLN,
                Name = x.Name,
                Price = x.Price,//TO ma być w groszach
                Quantity = x.Quantity,
            }).ToList()
        };
        var paymentUrl = await _stripeService.CreateCheckoutSession(checkoutSession);
        //TODO: obsługa błędu, retry, empty Order created. Powinno zwrócić też id

        await _orderRepository.Add(newOrder);

        await _integrationEventPublisher.Publish(new CreatedOrderEvent(
            OrderId: newOrder.Id,
            TotalAmount: newOrder.TotalPrice,
            Currency: Currency.PLN));

        await _unitOfWork.SaveChanges();
    }
}