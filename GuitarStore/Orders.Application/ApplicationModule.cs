using Application.CQRS.Command;
using Application.CQRS.Query;
using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Orders.BackgroundJobs;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Events.Incoming;
using System.Reflection;
using System.Runtime.CompilerServices;
using Common.EfCore.Transactions;
using Orders.Application.Abstractions;

[assembly: InternalsVisibleTo("Orders.Infrastructure")]
namespace Orders.Application;

internal static class ApplicationModule
{
    internal static void AddApplicationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<ICommandHandler<PlaceOrderResponse, PlaceOrderCommand>, PlaceOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelOrderCommand>, CancelOrderCommandHandler>();

        services.AddSingleton<IEventBusSubscriptionManager, EventBusSubscriptionManager>();
        services.AddScoped<IIntegrationEventHandler<OrderPaidEvent>, OrderPaidEventHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderPaymentFailedEvent>, OrderPaymentFailedEventHandler>();

        services.AddHostedService<OrderExpirationJob>();

        services.Configure<Configuration.OrdersConfiguration>(configuration.GetSection("Orders"));
    }
}
