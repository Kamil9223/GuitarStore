using Microsoft.EntityFrameworkCore;

namespace Common.EfCore.Transactions;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}

public class UnitOfWork<TContext>(TContext dbContext) : IUnitOfWork
    where TContext : DbContext
{
    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}
