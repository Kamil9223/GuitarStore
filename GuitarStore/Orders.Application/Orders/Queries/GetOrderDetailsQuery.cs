using Application.CQRS.Query;
using Domain.StronglyTypedIds;

namespace Orders.Application.Orders.Queries;
public sealed record GetOrderDetailsQuery(OrderId OrderId) : IQuery;

public sealed record OrderDetailsResponse
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required byte Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required decimal TotalPrice { get; init; }
    public required int ItemsCount { get; init; }
    public required Address DeliveryAddress { get; init; }
    public required string Deliverer { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required IReadOnlyCollection<OrderItem> Items { get; init; }

    public sealed record Address
    {
        public required string Country { get; init; }
        public required string LocalityName { get; init; }
        public required string PostalCode { get; init; }
        public required string HouseNumber { get; init; }
        public required string Street { get; init; }
        public required string? LocalNumber { get; init; }
    }

    public sealed record OrderItem
    {
        public required Guid Id { get; init; }
        public required string ProductName { get; init; }
        public required string Name { get; init; }
        public required decimal Price { get; init; }
        public required int Quantity { get; init; }
    }
}

public sealed class GetOrderDetailsQueryHandler(IOrderQueryService orderQueryService)
    : IQueryHandler<GetOrderDetailsQuery, OrderDetailsResponse>
{
    public Task<OrderDetailsResponse> Handle(GetOrderDetailsQuery query, CancellationToken ct)
    {
        return orderQueryService.GetOrderDetails(query.OrderId, ct);
    }
}
