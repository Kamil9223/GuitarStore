using Autofac;
using Warehouse.Infrastructure.Configuration;

namespace GuitarStore.ApiGateway.Configuration;

internal class ModulesInitializator : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<WarehouseModuleInitializator>();
    }
}
