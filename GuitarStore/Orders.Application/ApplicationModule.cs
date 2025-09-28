using Application.CQRS.Command;
using Application.CQRS.Query;
using Common.EfCore.Transactions;
using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Events.Incoming;
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

        services.AddSingleton<IEventBusSubscriptionManager, EventBusSubscriptionManager>();
        services.AddScoped<IIntegrationEventHandler<OrderPaidEvent>, OrderPaidEventHandler>();
    }
}
