using Application.CQRS;
using Autofac;
using Microsoft.Extensions.Hosting;
using Orders.Application.Orders.BackgroundJobs;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Orders.Infrastructure")]
namespace Orders.Application;

internal sealed class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(ICommandHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
           .AsClosedTypesOf(typeof(IQueryHandler<,>))
           .AsImplementedInterfaces()
           .InstancePerLifetimeScope();

        builder.RegisterType<OrderCompletionChannel>()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<OrderCompletionJob>()
            .As<BackgroundService>()
            .SingleInstance();
    }
}
