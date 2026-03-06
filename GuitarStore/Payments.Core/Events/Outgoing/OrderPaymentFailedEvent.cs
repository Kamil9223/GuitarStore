using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Payments.Core.Events.Outgoing;

/// <summary>
/// Published when a payment attempt fails (non-terminal).
/// The order remains in PendingPayment status and can be retried within TTL.
/// </summary>
public sealed record OrderPaymentFailedEvent(
    OrderId OrderId,
    string PaymentIntentId,
    string? FailureCode,
    DateTime OccurredAtUtc
) : IntegrationEvent, IIntegrationPublishEvent;
