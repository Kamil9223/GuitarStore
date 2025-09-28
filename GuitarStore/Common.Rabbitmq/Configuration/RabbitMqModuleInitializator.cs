using Microsoft.Extensions.DependencyInjection;
using Common.RabbitMq.Extensions;

namespace Common.RabbitMq.Configuration;
public static class RabbitMqModuleInitializator
{
    public static IServiceCollection AddRabbitMqModule(this IServiceCollection services)
    {
        services.AddRabbitMq();
        services.RegisterRabbitMqBackgroundWorkers();
        return services;
    }
}
