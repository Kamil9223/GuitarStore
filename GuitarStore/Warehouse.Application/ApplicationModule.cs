using Autofac;
using System.Reflection;
using System.Runtime.CompilerServices;
using Module = Autofac.Module;

[assembly: InternalsVisibleTo("Warehouse.Infrastructure")]
namespace Warehouse.Application;

internal sealed class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => type.Name.EndsWith("Service"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
