using Domain.ValueObjects;

namespace Payments.Shared.Contracts;
public sealed record CheckoutSessionRequest
{
    public IReadOnlyCollection<ProductItem> Products { get; init; } = new List<ProductItem>();

    public sealed record ProductItem
    {
        public decimal Amount { get; init; }
        public Currency Currency { get; init; } = null!;
        public long Quantity { get; init; }
        public string Name { get; init; } = null!;
    }
}

public sealed record CheckoutSessionResponse
{
    public string Url { get; init; } = null!;
    public string SessionId { get; set; } = null!;
}

