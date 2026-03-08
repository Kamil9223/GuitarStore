using System.Transactions;

namespace Common.EfCore.Transactions;

public class CrossModuleTransactionExecutor : ICrossModuleTransactionExecutor
{
    public async Task ExecuteAsync(Func<Task> operation, CancellationToken ct)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await operation();
        scope.Complete();
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken ct)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var result = await operation();
        scope.Complete();
        return result;
    }
}
