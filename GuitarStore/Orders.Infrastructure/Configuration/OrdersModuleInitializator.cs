using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application;

namespace Orders.Infrastructure.Configuration;
public static class OrdersModuleInitializator
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureModule(configuration);
        services.AddApplicationModule();
        return services;
    }
}
