using Domain.StronglyTypedIds;
using Orders.Application.Orders.Queries;

namespace Orders.Application.Orders;
public interface IOrderQueryService
{
    Task<OrderStatusResponse> GetOrderStatus(OrderId orderId, CancellationToken ct);

    Task<OrderDetailsResponse> GetOrderDetails(OrderId orderId, CancellationToken ct);

    Task<OrdersHistoryResponse> GetOrdersHistory(CustomerId customerId, CancellationToken ct);
}
