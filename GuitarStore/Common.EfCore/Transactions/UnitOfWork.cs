using Microsoft.EntityFrameworkCore;

namespace Common.EfCore.Transactions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}

public class UnitOfWork<TContext>(TContext dbContext) : IUnitOfWork
    where TContext : DbContext
{
    public async Task SaveChangesAsync(CancellationToken ct) => await dbContext.SaveChangesAsync(ct);
}
