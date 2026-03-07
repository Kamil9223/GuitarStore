using Common.Errors.Exceptions;
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

    public async Task<Order> Get(OrderId orderId, CancellationToken ct)
    {
        var dbOrder = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, ct);
        if (dbOrder is null)
            throw new NotFoundException(orderId);

        var settings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        var order = JsonConvert.DeserializeObject<Order>(dbOrder.Object, settings)!;
        return order;
    }

    public async Task Add(Order order, CancellationToken ct)
    {
        var orderDbModel = new OrderDbModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ExpiresAtUtc = order.ExpiresAtUtc,
            Object = JsonConvert.SerializeObject(order)
        };

        await _dbContext.Orders.AddAsync(orderDbModel, ct);
    }

    public async Task Update(Order order, CancellationToken ct)
    {
        var dbOrder = await _dbContext.Orders.SingleAsync(x => x.Id == order.Id, ct);
        var settings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };
        dbOrder.ExpiresAtUtc = order.ExpiresAtUtc;
        dbOrder.Object = JsonConvert.SerializeObject(order, settings);
    }

    public async Task<IReadOnlyCollection<Order>> GetExpiredPendingPaymentOrders(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var dbOrders = await _dbContext.Orders
            .Where(x => x.ExpiresAtUtc < now)
            .ToListAsync(ct);

        var settings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        var orders = dbOrders
            .Select(dbOrder => JsonConvert.DeserializeObject<Order>(dbOrder.Object, settings)!)
            .Where(order => order.Status == OrderStatus.PendingPayment)
            .ToList();

        return orders;
    }
}
