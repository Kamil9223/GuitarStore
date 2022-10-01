using Autofac;
using Infrastructure.Configuration;
using Warehouse.Infrastructure.Configuration;

namespace GuitarStore.ApiGateway.Configuration;

internal class ModulesInitializator : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<CommonModule>();
        builder.RegisterModule<WarehouseModuleInitializator>();
        builder.RegisterModule<ApiModule>();
    }
}
