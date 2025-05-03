using GuitarStore.ApiGateway.MiddleWares;

namespace GuitarStore.ApiGateway.Configuration;

internal static class ApiModule
{
    public static IServiceCollection AddApiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ExceptionsMiddleware>();

        return services;
    }
}
