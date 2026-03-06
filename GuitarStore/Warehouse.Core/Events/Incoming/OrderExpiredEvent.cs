using Common.RabbitMq.Abstractions.Events;
using Common.RabbitMq.Abstractions.EventHandlers;
using Domain.StronglyTypedIds;
using Microsoft.Extensions.Logging;
using Warehouse.Shared;

namespace Warehouse.Core.Events.Incoming;

internal sealed record OrderExpiredEvent(
    OrderId OrderId,
    string Reason,
    DateTime OccurredAtUtc
) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class OrderExpiredEventHandler : IIntegrationEventHandler<OrderExpiredEvent>
{
    private readonly IProductReservationService _reservationService;
    private readonly ILogger<OrderExpiredEventHandler> _logger;

    public OrderExpiredEventHandler(
        IProductReservationService reservationService,
        ILogger<OrderExpiredEventHandler> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    public async Task Handle(OrderExpiredEvent @event, CancellationToken ct)
    {
        _logger.LogInformation(
            "Order {OrderId} expired. Reason: {Reason}. Releasing product reservations.",
            @event.OrderId, @event.Reason);

        await _reservationService.ReleaseReservations(@event.OrderId, ct);

        _logger.LogInformation(
            "Product reservations for Order {OrderId} released successfully. Stock restored.",
            @event.OrderId);
    }
}
