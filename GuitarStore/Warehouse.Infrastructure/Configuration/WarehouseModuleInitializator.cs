using Autofac;
using Warehouse.Application;

namespace Warehouse.Infrastructure.Configuration;

public sealed class WarehouseModuleInitializator : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<InfrastructureModule>();
        builder.RegisterModule<ApplicationModule>();
    }
}
