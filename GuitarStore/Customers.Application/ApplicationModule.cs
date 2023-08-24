using Application.RabbitMq.Abstractions;
using Autofac;
using System.Reflection;
using System.Runtime.CompilerServices;
using Module = Autofac.Module;

[assembly: InternalsVisibleTo("Customers.Infrastructure")]
namespace Customers.Application;

internal sealed class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<EventBusSubscriptionManager>()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
