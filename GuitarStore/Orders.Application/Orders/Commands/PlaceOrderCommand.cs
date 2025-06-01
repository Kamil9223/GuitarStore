using Application.CQRS;
using Customers.Shared;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Orders.Application.Abstractions;
using Orders.Domain.Orders;
using Payments.Shared.Contracts;
using Payments.Shared.Services;
using Warehouse.Shared;

namespace Orders.Application.Orders.Commands;

public sealed record PlaceOrderCommand(CustomerId CustomerId) : ICommand;

public sealed record PlaceOrderResponse(string PaymentUrl, string SessionId);

internal sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderResponse, PlaceOrderCommand>
{
    private readonly ICartService _cartService;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;
    private readonly IProductReservationService _productReservationService;
    private readonly IStripeService _stripeService;

    public PlaceOrderCommandHandler(
        ICartService cartService,
        IOrderRepository orderRepository,
        IOrdersUnitOfWork unitOfWork,
        IProductReservationService productReservationService,
        IStripeService stripeService)
    {
        _cartService = cartService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _productReservationService = productReservationService;
        _stripeService = stripeService;
    }

    public async Task<PlaceOrderResponse> Handle(PlaceOrderCommand command)
    {
        var checkoutCart = await _cartService.GetCheckoutCart(command.CustomerId);

        var newOrder = Order.Create(
            orderItems: OrdersMapper.MapToOrderItems(checkoutCart.Items),
            customerId: checkoutCart.CustomerId,
            deliveryAddress: OrdersMapper.MapToDeliveryAddress(checkoutCart.DeliveryAddress),
            delivery: new Delivery(checkoutCart.DelivererId, checkoutCart.Deliverer));

        await _productReservationService.ReserveProduct(OrdersMapper.MapToReserveProductsDto(newOrder));

        var checkoutSession = new CheckoutSessionRequest
        {
            Products = checkoutCart.Items.Select(x => new CheckoutSessionRequest.ProductItem
            {
                Currency = Currency.PLN,
                Name = x.Name,
                Amount = x.Amount * 100,
                Quantity = x.Quantity,
            }).ToList()
        };
        var session = await _stripeService.CreateCheckoutSession(checkoutSession);

        await _orderRepository.Add(newOrder);
        await _unitOfWork.SaveChangesAsync();
        return new PlaceOrderResponse(session.Url, session.SessionId);
    }
}