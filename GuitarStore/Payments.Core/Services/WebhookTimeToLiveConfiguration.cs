namespace Payments.Core.Services;

public sealed record WebhookTimeToLiveConfiguration
{
    /// <summary>
    /// Stripe webhook time to live configuration (in hours)
    /// </summary>
    public int Ttl { get; init; }
    
    public bool IsExpired(DateTimeOffset createdUtc) => createdUtc.AddHours(Ttl) < DateTimeOffset.UtcNow;
}