namespace Common.EfCore.Transactions;

/// <summary>
/// Executes operations within a cross-module distributed transaction (TransactionScope).
/// Use when operation spans multiple DbContexts from different modules.
/// </summary>
public interface ICrossModuleTransactionExecutor
{
    /// <summary>
    /// Executes the operation within a distributed transaction (TransactionScope).
    /// Commits on success, rolls back on exception.
    /// </summary>
    Task ExecuteAsync(Func<Task> operation, CancellationToken ct);

    /// <summary>
    /// Executes the operation within a distributed transaction (TransactionScope) and returns a result.
    /// Commits on success, rolls back on exception.
    /// </summary>
    Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken ct);
}
