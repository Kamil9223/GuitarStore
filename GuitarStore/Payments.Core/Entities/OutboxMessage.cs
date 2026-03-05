namespace Payments.Core.Entities;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DateTime OccurredOnUtc { get; init; }
    public string? CorrelationId { get; init; }
    public DateTime? ProcessedOnUtc { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }

    private OutboxMessage() { } // EF

    public OutboxMessage(string type, string payload, string? correlationId = null)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        OccurredOnUtc = DateTime.UtcNow;
        CorrelationId = correlationId;
        RetryCount = 0;
    }

    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
    }

    public void IncrementRetryCount(string? error = null)
    {
        RetryCount++;
        LastError = error;
    }
}
