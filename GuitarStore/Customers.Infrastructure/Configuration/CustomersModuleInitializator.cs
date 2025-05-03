using Customers.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Customers.Infrastructure.Configuration;

public static class CustomersModuleInitializator
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureModule(configuration);
        services.AddApplicationModule();
        return services;
    }
}
