using Domain.StronglyTypedIds;

namespace Orders.Domain.Orders;
public interface IOrderRepository
{
    Task<Order> Get(OrderId orderId);
    Task Add(Order order);
}
