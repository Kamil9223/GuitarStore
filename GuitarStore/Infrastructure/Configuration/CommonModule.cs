using Autofac;
using Infrastructure.RabbitMq.Extensions;

namespace Infrastructure.Configuration;

public sealed class CommonModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<Application.Configuration.CommonModule>();
        builder.RabbitMqConnection();
    }
}
