using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Payments.Shared.Contracts;
public sealed record CheckoutSessionRequest
{
    public required OrderId OrderId { get; init; }
    public required IReadOnlyCollection<ProductItem> Products { get; init; }

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

