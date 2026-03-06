using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Orders.Application.Orders.Events.Outgoing;

/// <summary>
/// Published when an order expires (terminal event).
/// This is a business decision made by the expiration job.
/// Order transitions to Cancelled status.
/// Reasons: "ReservationExpired", "PaymentTimeout"
/// </summary>
public sealed record OrderExpiredEvent(
    OrderId OrderId,
    string Reason, // "ReservationExpired", "PaymentTimeout"
    DateTime OccurredAtUtc
) : IntegrationEvent, IIntegrationPublishEvent;
