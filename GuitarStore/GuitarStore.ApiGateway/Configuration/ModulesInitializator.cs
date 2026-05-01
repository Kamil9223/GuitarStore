using Application.Configuration;
using Auth.Core;
using Catalog.Infrastructure.Configuration;
using Common.Outbox;
using Common.RabbitMq.Configuration;
using Customers.Infrastructure.Configuration;
using Delivery.Core;
using Orders.Infrastructure.Configuration;
using Payments.Core;
using Warehouse.Core;

namespace GuitarStore.ApiGateway.Configuration;

internal static class ModulesInitializator
{
    public static IServiceCollection InitializeModules(this IServiceCollection services, IConfiguration configuration, bool skipHostedServices = false)
    {
        services
            .AddApplicationModule()
            .AddAuthModule(configuration)
            .AddRabbitMqModule(skipHostedServices)
            .AddOutbox(configuration, skipHostedServices)
            .AddCatalogModule(configuration)
            .AddCustomersModule(configuration)
            .AddOrdersModule(configuration, skipHostedServices)
            .AddWarehouseModule(configuration, skipHostedServices)
            .AddPaymentsModule(configuration)
            .AddDeliveryModule(configuration)
            .AddApiModule(configuration);

        return services;
    }

    public static async Task InitializeModulesAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await app.Services.InitializeAuthModuleAsync(cancellationToken);
    }
}
