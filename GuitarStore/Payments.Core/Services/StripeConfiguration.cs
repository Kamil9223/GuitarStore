namespace Payments.Core.Services;

public sealed record StripeConfiguration
{
    public string WebhookSecret { get; init; }
}