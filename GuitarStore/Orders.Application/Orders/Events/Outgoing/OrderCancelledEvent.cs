using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Orders.Application.Orders.Events.Outgoing;

/// <summary>
/// Published when an order is cancelled (business decision).
/// This is a terminal event - order transitions to Cancelled status.
/// Reasons: User cancellation, Admin cancellation.
/// NOT published for payment failures - see OrderPaymentFailedEvent.
/// </summary>
public sealed record OrderCancelledEvent(
    OrderId OrderId,
    string Reason,
    string CancelledBy, // "User", "System", "Admin"
    DateTime OccurredAtUtc
) : IntegrationEvent, IIntegrationPublishEvent;
