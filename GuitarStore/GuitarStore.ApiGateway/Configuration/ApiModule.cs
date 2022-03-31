using Autofac;
using GuitarStore.ApiGateway.MiddleWares;

namespace GuitarStore.ApiGateway.Configuration;

internal class ApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ExceptionsMiddleware>().AsSelf().InstancePerDependency();
    }
}
