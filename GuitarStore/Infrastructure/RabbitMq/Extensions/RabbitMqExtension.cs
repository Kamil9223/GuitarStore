using Autofac;
using Infrastructure.RabbitMq.Implementations;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.RabbitMq.Extensions;

internal static class RabbitMqExtension
{
    public static ContainerBuilder RabbitMqConnection(this ContainerBuilder builder)
    {
        builder.RegisterType<RabbitMqConnector>()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<IntegrationEventPublisher>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterType<IntegrationEventSubscriber>()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<IntegrationEventsSubscriptionManager>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<RabbitMqSetupBackgroundService>()
            .As<IHostedService>()
            .SingleInstance();

        builder.RegisterType<RabbitMqSubscriptionBackgroundService>()
            .As<IHostedService>()
            .SingleInstance();

        return builder;
    }
}
