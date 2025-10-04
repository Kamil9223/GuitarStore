using Application.CQRS.Query;
using Customers.Domain.Carts;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Customers.Application.Carts.Queries;
public sealed record GetCartQuery(CustomerId CustomerId) : IQuery;

public sealed record CartDetailsResponse
{
    public required Guid CustomerId { get; init; }
    public required decimal TotalPrice { get; init; }
    public required CartState CartState { get; set; }
    public required int ItemsCount { get; init; }
    public required string? Deliverer { get; init; }

    public IReadOnlyCollection<CartItem> Items { get; init; } = [];

    public sealed record CartItem
    {
        public required Guid ProductId { get; init; }
        public required string ProductName { get; init; }
        public required decimal Price { get; init; }
        public required int Quantity { get; set; }
    }
}

public sealed class GetCartQueryHandler(ICartQueryService cartQueryService)
    : IQueryHandler<GetCartQuery, CartDetailsResponse>
{
    public async Task<CartDetailsResponse> Handle(GetCartQuery query, CancellationToken ct)
    {
        return await cartQueryService.GetCartDetails(query.CustomerId, ct);
    }
}
