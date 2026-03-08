using Common.EfCore.DbContext;

namespace Common.EfCore.Transactions;

public class TransactionExecutor<TUnitOfWork>(TUnitOfWork unitOfWork, IDbContext dbContext)
    : ITransactionExecutor<TUnitOfWork>
    where TUnitOfWork : IUnitOfWork
{
    public async Task ExecuteAsync(Func<TUnitOfWork, Task> operation, CancellationToken ct)
    {
        await using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            await operation(unitOfWork);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<TUnitOfWork, Task<TResult>> operation, CancellationToken ct)
    {
        await using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            var result = await operation(unitOfWork);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
