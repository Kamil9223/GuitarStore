using Customers.Domain.Carts;

namespace Customers.Infrastructure.Carts;
public sealed class CartReadModel
{
    public required Guid CustomerId { get; init; }
    public required CartState CartState { get; set; }
    public required decimal TotalPrice { get; init; }
    public required int ItemsCount { get; init; }
    public required string? Deliverer { get; init; }
}

public sealed class CartItemReadModel
{
    public required Guid CustomerId { get; init; }
    public required Guid ProductId { get; init; }
}
