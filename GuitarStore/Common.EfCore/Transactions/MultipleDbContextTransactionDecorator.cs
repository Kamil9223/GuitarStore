using Application.CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Common.EfCore.Transactions;
public class MultipleDbContextTransactionDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly IReadOnlyCollection<DbContext> _dbContexts;

    public MultipleDbContextTransactionDecorator(ICommandHandler<TCommand> inner, IReadOnlyCollection<DbContext> dbContexts)
    {
        _inner = inner;
        _dbContexts = dbContexts;
    }

    public async Task Handle(TCommand command)
    {
        var primary = _dbContexts.First();
        await using var transaction = await primary.Database.BeginTransactionAsync();

        try
        {
            var dbTransaction = primary.Database.CurrentTransaction!.GetDbTransaction();

            foreach (var ctx in _dbContexts.Skip(1))
            {
                await ctx.Database.UseTransactionAsync(dbTransaction);
            }

            await _inner.Handle(command);

            foreach (var ctx in _dbContexts)
            {
                await ctx.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
