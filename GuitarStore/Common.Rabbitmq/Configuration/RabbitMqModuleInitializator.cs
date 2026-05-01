using Microsoft.Extensions.DependencyInjection;
using Common.RabbitMq.Extensions;

namespace Common.RabbitMq.Configuration;
public static class RabbitMqModuleInitializator
{
    public static IServiceCollection AddRabbitMqModule(this IServiceCollection services, bool skipHostedServices = false)
    {
        services.AddRabbitMq();
        if (!skipHostedServices)
            services.RegisterRabbitMqBackgroundWorkers();
        return services;
    }
}
