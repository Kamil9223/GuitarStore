using Autofac;
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

        return builder;
    }
}
