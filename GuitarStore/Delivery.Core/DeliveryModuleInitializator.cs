using Application.RabbitMq.Abstractions;
using Autofac;
using System.Reflection;

namespace Delivery.Core;
public sealed class DeliveryModuleInitializator : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
