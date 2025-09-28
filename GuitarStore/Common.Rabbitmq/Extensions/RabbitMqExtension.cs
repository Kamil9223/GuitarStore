using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Common.RabbitMq.Extensions;

internal static class RabbitMqExtension
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMqConnector>();
        services.AddSingleton<IRabbitMqConnector>(provider =>
            provider.GetRequiredService<RabbitMqConnector>());
        services.AddSingleton<IRabbitMqChannel>(provider =>
            provider.GetRequiredService<RabbitMqConnector>());

        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        services.AddSingleton<IIntegrationEventSubscriber, IntegrationEventSubscriber>();

        return services;
    }

    public static IServiceCollection RegisterRabbitMqBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<RabbitMqSetupBackgroundService>();
        services.AddHostedService<RabbitMqSubscriptionBackgroundService>();
        return services;
    }
}
