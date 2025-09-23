using Application.CQRS.Command;
using Microsoft.Extensions.DependencyInjection;

namespace Common.EfCore.Transactions;
public static class TransactionalDecoratorExtensions
{
    public static IServiceCollection AddTransactionalDecorator<TCommand>(
        this IServiceCollection services,
        Func<IServiceProvider, IReadOnlyCollection<IDbContext>> resolveDbContexts)
        where TCommand : class, ICommand
    {
        services.Decorate<ICommandHandler<TCommand>>((inner, sp) =>
        {
            IReadOnlyCollection<IDbContext> factory() => resolveDbContexts(sp);
            return new MultipleDbContextTransactionDecorator<TCommand>(inner, factory);
        });

        return services;
    }

    public static IServiceCollection AddTransactionalDecorator<TResponse, TCommand>(
        this IServiceCollection services,
        Func<IServiceProvider, IReadOnlyCollection<IDbContext>> resolveDbContexts)
        where TCommand : class, ICommand
    {
        services.Decorate<ICommandHandler<TResponse, TCommand>>((inner, sp) =>
        {
            IReadOnlyCollection<IDbContext> factory() => resolveDbContexts(sp);
            return new MultipleDbContextTransactionDecorator<TResponse, TCommand>(inner, factory);
        });

        return services;
    }
}
