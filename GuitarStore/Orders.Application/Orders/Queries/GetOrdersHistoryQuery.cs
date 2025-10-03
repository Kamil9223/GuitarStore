using Application.CQRS.Query;
using Domain.StronglyTypedIds;

namespace Orders.Application.Orders.Queries;
public sealed record GetOrdersHistoryQuery(CustomerId CustomerId) : IQuery;

public sealed record OrdersHistoryResponse
{
    public required IReadOnlyCollection<OrderHistoryItem> Items { get; init; }

    public sealed record OrderHistoryItem
    {
        public required Guid Id { get; init; }
        public required Guid CustomerId { get; init; }
        public required byte Status { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required decimal TotalPrice { get; init; }
        public required int ItemsCount { get; init; }
        public required string Deliverer { get; init; }
        public required DateTime UpdatedAt { get; init; }
    }
}

public sealed class GetOrdersHistoryQueryHandler(IOrderQueryService orderQueryService)
    : IQueryHandler<GetOrdersHistoryQuery, OrdersHistoryResponse>
{
    public Task<OrdersHistoryResponse> Handle(GetOrdersHistoryQuery query, CancellationToken ct)
    {
        return orderQueryService.GetOrdersHistory(query.CustomerId, ct);
    }
}
