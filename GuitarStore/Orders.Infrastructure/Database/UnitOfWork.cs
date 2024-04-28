using Microsoft.EntityFrameworkCore.Storage;
using Orders.Application.Abstractions;
using System.Data;

namespace Orders.Infrastructure.Database;
internal class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _dbContext;

    public UnitOfWork(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChanges()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IDbTransaction> BeginTransaction()
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();

        return transaction.GetDbTransaction();
    }
}
