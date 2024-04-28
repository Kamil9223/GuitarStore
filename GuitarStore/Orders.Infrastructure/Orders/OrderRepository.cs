using Newtonsoft.Json;
using Orders.Domain.Orders;
using Orders.Infrastructure.Database;

namespace Orders.Infrastructure.Orders;
internal class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;

    public OrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(Order order)
    {
        var orderDbModel = new OrderDbModel
        {
            CustomerId = order.CustomerId,
            Object = JsonConvert.SerializeObject(order)
        };

        await _dbContext.Orders.AddAsync(orderDbModel);
    }
}
