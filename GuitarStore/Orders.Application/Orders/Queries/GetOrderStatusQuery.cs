using Application.CQRS.Query;
using Domain.StronglyTypedIds;

namespace Orders.Application.Orders.Queries;
public sealed record GetOrderStatusQuery(OrderId OrderId) : IQuery;

public sealed record OrderStatusResponse
{
    public required byte Status { get; init; }
};

internal sealed class GetOrderStatusQueryHandler(IOrderQueryService orderQueryService)
    : IQueryHandler<GetOrderStatusQuery, OrderStatusResponse>
{
    public Task<OrderStatusResponse> Handle(GetOrderStatusQuery query, CancellationToken ct)
    {
        return orderQueryService.GetOrderStatus(query.OrderId, ct);
    }
}
