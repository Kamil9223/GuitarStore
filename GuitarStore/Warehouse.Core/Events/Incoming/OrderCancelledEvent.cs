using Common.RabbitMq.Abstractions.Events;
using Common.RabbitMq.Abstractions.EventHandlers;
using Domain.StronglyTypedIds;
using Microsoft.Extensions.Logging;
using Warehouse.Shared;

namespace Warehouse.Core.Events.Incoming;

internal sealed record OrderCancelledEvent(
    OrderId OrderId,
    string Reason,
    string CancelledBy,
    DateTime OccurredAtUtc
) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class OrderCancelledEventHandler : IIntegrationEventHandler<OrderCancelledEvent>
{
    private readonly IProductReservationService _reservationService;
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(
        IProductReservationService reservationService,
        ILogger<OrderCancelledEventHandler> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    public async Task Handle(OrderCancelledEvent @event, CancellationToken ct)
    {
        _logger.LogInformation(
            "Order {OrderId} cancelled. Reason: {Reason}, CancelledBy: {CancelledBy}. Releasing product reservations.",
            @event.OrderId, @event.Reason, @event.CancelledBy);

        await _reservationService.ReleaseReservations(@event.OrderId, ct);

        _logger.LogInformation(
            "Product reservations for Order {OrderId} released successfully. Stock restored.",
            @event.OrderId);
    }
}
