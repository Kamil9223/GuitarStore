namespace Common.EfCore.Transactions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}

public class UnitOfWork<TContext>(TContext dbContext) : IUnitOfWork
    where TContext : Microsoft.EntityFrameworkCore.DbContext
{
    public async Task SaveChangesAsync(CancellationToken ct) => await dbContext.SaveChangesAsync(ct);
}
