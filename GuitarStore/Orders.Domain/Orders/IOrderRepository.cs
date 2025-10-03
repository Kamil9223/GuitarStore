using Domain.StronglyTypedIds;

namespace Orders.Domain.Orders;
public interface IOrderRepository
{
    Task<Order> Get(OrderId orderId, CancellationToken ct);
    Task Add(Order order, CancellationToken ct);
    Task Update(Order order, CancellationToken ct);
}
