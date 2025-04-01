using Application.Exceptions;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Order> Get(OrderId orderId)
    {
        var dbOrder = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
        if (dbOrder is null)
            throw new NotFoundException($"Order with Id: {orderId} not exists.");

        var order = JsonConvert.DeserializeObject<Order>(dbOrder.Object)!;
        return order;
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
