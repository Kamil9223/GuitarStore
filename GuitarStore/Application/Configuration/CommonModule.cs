using Application.CQRS;
using Autofac;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Infrastructure")]
namespace Application.Configuration;
internal class CommonModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandHandlerExecutor>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<QueryHandlerExecutor>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}
