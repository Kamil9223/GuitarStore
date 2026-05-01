namespace Common.Outbox;
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    
    private OutboxMessage() { }
    
    public OutboxMessage(string type, string payload, string? correlationId = null)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        OccurredOnUtc = DateTime.UtcNow;
        CorrelationId = correlationId;
        RetryCount = 0;
    }
    
    public void MarkAsProcessed() => ProcessedOnUtc = DateTime.UtcNow;

    public void IncrementRetryCount(string? error = null)
    {
        RetryCount++;
        LastError = error;
    }
}
