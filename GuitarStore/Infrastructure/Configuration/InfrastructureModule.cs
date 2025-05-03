using Infrastructure.RabbitMq.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

internal static class InfrastructureModule
{
    internal static IServiceCollection AddInfrastructureModule(this IServiceCollection services)
    {
        services.AddRabbitMq();
        services.RegisterRabbitMqBackgroundWorkers();
        return services;
    }
}
