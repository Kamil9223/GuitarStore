using Domain.ValueObjects;

namespace Payments.Shared.Contracts;
public sealed record CheckoutSessionRequest
{
    public IReadOnlyCollection<ProductItem> Products { get; init; } = new List<ProductItem>();

    public sealed record ProductItem
    {
        public decimal Price { get; init; }
        public Currency Currency { get; init; } = null!;
        public long Quantity { get; init; }
        public string Name { get; init; } = null!;
    }
}
