using Common.RabbitMq.Abstractions.Events;
using Common.RabbitMq.Abstractions.EventHandlers;
using Domain.StronglyTypedIds;
using Microsoft.Extensions.Logging;
using Warehouse.Shared;

namespace Warehouse.Core.Events.Incoming;

internal sealed record OrderPaidEvent(OrderId OrderId)
    : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class OrderPaidEventHandler : IIntegrationEventHandler<OrderPaidEvent>
{
    private readonly IProductReservationService _reservationService;
    private readonly ILogger<OrderPaidEventHandler> _logger;

    public OrderPaidEventHandler(
        IProductReservationService reservationService,
        ILogger<OrderPaidEventHandler> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    public async Task Handle(OrderPaidEvent @event, CancellationToken ct)
    {
        _logger.LogInformation(
            "Order {OrderId} paid. Confirming product reservations.",
            @event.OrderId);

        await _reservationService.ConfirmReservations(@event.OrderId, ct);

        _logger.LogInformation(
            "Product reservations for Order {OrderId} confirmed successfully.",
            @event.OrderId);
    }
}
