using Common.EfCore.Transactions;
using Orders.Application.Abstractions;

namespace Orders.Infrastructure.Database;
internal class OrdersUnitOfWork(OrdersDbContext dbContext)
    : UnitOfWork<OrdersDbContext>(dbContext), IOrdersUnitOfWork
{
}
