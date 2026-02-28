using Domain;

namespace Payments.Core.Entities;

public class ProcessedWebhookMessage : Entity
{
    public Guid Id { get; init; }
    public string EventId { get; init; } = default!;
    public string EventType { get; init; } = default!;
    public string? OrderId { get; init; }
    public DateTimeOffset ReceivedAtUtc { get; init; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public WebhookProcessingStatus Status { get; set; } = WebhookProcessingStatus.Processing;
    public string? Error { get; set; }

    private ProcessedWebhookMessage() { } // EF

    public ProcessedWebhookMessage(string eventId, string eventType, string? orderId)
    {
        EventId = eventId;
        EventType = eventType;
        OrderId = orderId;
        ReceivedAtUtc = DateTimeOffset.UtcNow;
    }
}

public enum WebhookProcessingStatus : byte
{
    Processing = 0,
    Completed = 1,
    Failed = 2
}