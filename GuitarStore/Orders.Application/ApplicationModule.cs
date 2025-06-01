using Application.CQRS;
using Common.EfCore.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Orders.Commands;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Orders.Infrastructure")]
namespace Orders.Application;

internal static class ApplicationModule
{
    internal static void AddApplicationModule(
        this IServiceCollection services,
        Func<IServiceProvider, IReadOnlyCollection<IDbContext>> placeOrderCommandTransactionDbContextsFunc
        )
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<ICommandHandler<PlaceOrderResponse, PlaceOrderCommand>, PlaceOrderCommandHandler>();
        services.AddTransactionalDecorator<PlaceOrderResponse, PlaceOrderCommand>(placeOrderCommandTransactionDbContextsFunc);
    }
}
