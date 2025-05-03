using Catalog.Infrastructure.Configuration;
using Customers.Infrastructure.Configuration;
using Delivery.Core;
using Infrastructure.Configuration;
using Orders.Infrastructure.Configuration;
using Payments.Core;
using Warehouse.Core;

namespace GuitarStore.ApiGateway.Configuration;

internal static class ModulesInitializator
{
    public static IServiceCollection InitializeModules(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCommonModule()
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
