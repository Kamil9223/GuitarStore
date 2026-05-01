using Application.CQRS.Command;
using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Core.Commands;
using Warehouse.Core.Database;
using Warehouse.Core.Events;
using Warehouse.Core.Events.Incoming;
using Warehouse.Core.InternalModuleApi;
using Warehouse.Core.Services;
using Warehouse.Shared;

namespace Warehouse.Core;
public static class WarehouseModuleInitializator
{
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services, IConfiguration configuration, bool skipHostedServices = false)
    {
        services.AddScoped(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;

            var dbOptions = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new WarehouseDbContext(dbOptions);
        });
        services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());

        services.AddScoped<IProductReservationService, ProductReservationService>();

        services.AddScoped<ICommandHandler<IncreaseStockQuantityCommand>, IncreaseStockQuantityCommandHandler>();

        if (!skipHostedServices)
            services.AddHostedService<StockReservationExpirationJob>();

        services.AddSingleton<IEventBusSubscriptionManager, EventBusSubscriptionManager>();
        services.AddScoped<IIntegrationEventHandler<OrderPaidEvent>, OrderPaidEventHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderCancelledEvent>, OrderCancelledEventHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderExpiredEvent>, OrderExpiredEventHandler>();

        return services;
    }
}
