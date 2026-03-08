using Common.EfCore.DbContext;
using Common.EfCore.Transactions;

namespace Orders.Application.Abstractions;
public interface IOrdersDbContext : IDbContext
{
}
