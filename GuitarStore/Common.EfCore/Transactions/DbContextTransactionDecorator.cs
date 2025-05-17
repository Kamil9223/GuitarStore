using Application.CQRS;
using Microsoft.EntityFrameworkCore.Storage;

namespace Common.EfCore.Transactions;

public interface IDbContext //TODO extract to separate lib .Abstractions
{
    Task<IDbContextTransaction> BeginTransactionAsync();

    Task SaveChangesAsync();
}

public class DbContextTransactionDecorator<TContext, TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
    where TContext : IDbContext
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly TContext _dbContext;

    public DbContextTransactionDecorator(ICommandHandler<TCommand> inner, TContext dbContext)
    {
        _inner = inner;
        _dbContext = dbContext;
    }

    public async Task Handle(TCommand command)
    {
        await using var transaction = await _dbContext.BeginTransactionAsync();

        try
        {
            transaction.GetDbTransaction();

            await _inner.Handle(command);

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
