namespace Common.EfCore.Transactions;

/// <summary>
/// Executes operations within a single-context database transaction.
/// </summary>
public interface ITransactionExecutor<TUnitOfWork> where TUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Executes the operation within a transaction. Commits on success, rolls back on exception.
    /// </summary>
    Task ExecuteAsync(Func<TUnitOfWork, Task> operation, CancellationToken ct);

    /// <summary>
    /// Executes the operation within a transaction and returns a result. Commits on success, rolls back on exception.
    /// </summary>
    Task<TResult> ExecuteAsync<TResult>(Func<TUnitOfWork, Task<TResult>> operation, CancellationToken ct);
}
