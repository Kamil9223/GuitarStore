using Catalog.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure.Configuration;

public static class CatalogModuleInitializator
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureModule(configuration);
        services.AddApplicationModule();
        return services;
    }
}
