namespace Orders.Domain.Orders;
public interface IOrderRepository
{
    Task Add(Order order);
}
