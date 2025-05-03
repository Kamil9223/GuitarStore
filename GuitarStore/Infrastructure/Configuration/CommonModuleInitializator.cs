using Microsoft.Extensions.DependencyInjection;
using Application.Configuration;

namespace Infrastructure.Configuration;
public static class CommonModuleInitializator
{
    public static IServiceCollection AddCommonModule(this IServiceCollection services)
    {
        services.AddInfrastructureModule();
        services.AddApplicationModule();
        return services;
    }
}
