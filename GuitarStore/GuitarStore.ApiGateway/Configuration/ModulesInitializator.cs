using Application.Configuration;
using Auth.Core;
using Catalog.Infrastructure.Configuration;
using Common.RabbitMq.Configuration;
using Customers.Infrastructure.Configuration;
using Delivery.Core;
using Orders.Infrastructure.Configuration;
using Payments.Core;
using Warehouse.Core;

namespace GuitarStore.ApiGateway.Configuration;

internal static class ModulesInitializator
{
    public static IServiceCollection InitializeModules(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplicationModule()
            .AddAuthModule(configuration)
            .AddRabbitMqModule()
            .AddCatalogModule(configuration)
            .AddCustomersModule(configuration)
            .AddOrdersModule(configuration)
            .AddWarehouseModule(configuration)
            .AddPaymentsModule(configuration)
            .AddDeliveryModule(configuration)
            .AddApiModule(configuration);

        return services;
    }
}
