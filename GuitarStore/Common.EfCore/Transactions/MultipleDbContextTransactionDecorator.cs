using Application.CQRS.Command;
using System.Transactions;

namespace Common.EfCore.Transactions;

public delegate IReadOnlyCollection<IDbContext> ResolveDbContextsDelegate();

public class MultipleDbContextTransactionDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly ResolveDbContextsDelegate _resolveDbContexts;

    public MultipleDbContextTransactionDecorator(
        ICommandHandler<TCommand> inner,
        ResolveDbContextsDelegate resolveDbContexts)
    {
        _inner = inner;
        _resolveDbContexts = resolveDbContexts;
    }

    public async Task Handle(TCommand command)
    {
        var dbContexts = _resolveDbContexts().ToList();

        if (dbContexts.Count == 0)
        {
            await _inner.Handle(command);
            return;
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _inner.Handle(command);

        //foreach (var ctx in dbContexts)
        //{
        //    await ctx.SaveChangesAsync();
        //}

        scope.Complete();
    }
}

public class MultipleDbContextTransactionDecorator<TResponse, TCommand> : ICommandHandler<TResponse, TCommand>
    where TCommand : class, ICommand
{
    private readonly ICommandHandler<TResponse, TCommand> _inner;
    private readonly ResolveDbContextsDelegate _resolveDbContexts;

    public MultipleDbContextTransactionDecorator(
        ICommandHandler<TResponse, TCommand> inner,
        ResolveDbContextsDelegate resolveDbContexts)
    {
        _inner = inner;
        _resolveDbContexts = resolveDbContexts;
    }

    public async Task<TResponse> Handle(TCommand command)
    {
        var dbContexts = _resolveDbContexts().ToList();

        if (dbContexts.Count == 0)
        {
            return await _inner.Handle(command);
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var response = await _inner.Handle(command);

        //foreach (var ctx in dbContexts)
        //{
        //    await ctx.SaveChangesAsync();
        //}

        scope.Complete();
        return response;
    }
}
