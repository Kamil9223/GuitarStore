using Application.Configuration;
using Catalog.Infrastructure.Configuration;
using Common.RabbitMq.Configuration;
using Customers.Infrastructure.Configuration;
using Delivery.Core;
using Orders.Application.Abstractions;
using Orders.Infrastructure.Configuration;
using Payments.Core;
using Warehouse.Core;
using Warehouse.Core.Database;

namespace GuitarStore.ApiGateway.Configuration;

internal static class ModulesInitializator
{
    public static IServiceCollection InitializeModules(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplicationModule()
            .AddRabbitMqModule()
            .AddCatalogModule(configuration)
            .AddCustomersModule(configuration)
            .AddOrdersModule(
                configuration,
                placeOrderCommandTransactionDbContextsFunc: (sp) =>
                [
                    sp.GetRequiredService<IOrdersDbContext>(),
                    sp.GetRequiredService<IWarehouseDbContext>()
                ])
            .AddWarehouseModule(configuration)
            .AddPaymentsModule(configuration)
            .AddDeliveryModule(configuration)
            .AddApiModule(configuration);

        return services;
    }
}
