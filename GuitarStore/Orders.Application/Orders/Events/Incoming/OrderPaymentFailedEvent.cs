using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;
using Microsoft.Extensions.Logging;

namespace Orders.Application.Orders.Events.Incoming;

/// <summary>
/// Consumed when a payment attempt fails (non-terminal).
/// Order remains in PendingPayment - payment can be retried within reservation TTL.
/// </summary>
internal sealed record OrderPaymentFailedEvent(
    OrderId OrderId,
    string PaymentIntentId,
    string? FailureCode,
    DateTime OccurredAtUtc
) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class OrderPaymentFailedEventHandler : IIntegrationEventHandler<OrderPaymentFailedEvent>
{
    private readonly ILogger<OrderPaymentFailedEventHandler> _logger;

    public OrderPaymentFailedEventHandler(ILogger<OrderPaymentFailedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderPaymentFailedEvent @event, CancellationToken ct)
    {
        // Non-terminal event - log for monitoring but don't change order status
        // Order remains in PendingPayment and can be retried
        _logger.LogWarning(
            "Payment failed for Order {OrderId}. PaymentIntent: {PaymentIntentId}, FailureCode: {FailureCode}. Order remains PendingPayment.",
            @event.OrderId, @event.PaymentIntentId, @event.FailureCode);

        // TODO: Optionally store failure info in Order entity (e.g., LastPaymentFailure field)
        // TODO: Optionally emit metric for monitoring

        return Task.CompletedTask;
    }
}
