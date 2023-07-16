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

        builder.RegisterType<RabbitMqSetupBackgroundService>()
            .As<IHostedService>()
            .SingleInstance();

        builder.RegisterType<IntegrationEventPublisher>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterType<IntegrationEventSubscriber>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterType<IntegrationEventsSubscriptionManager>()
            .AsSelf()
            .SingleInstance();

        return builder;
    }
}
