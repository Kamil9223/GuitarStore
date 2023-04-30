using Autofac;
using Infrastructure.Configuration;
using Warehouse.Infrastructure.Configuration;

namespace GuitarStore.ApiGateway.Configuration;

internal class ModulesInitializator : Module
{
    private readonly IConfiguration _configuration;

    internal ModulesInitializator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<CommonModule>();
        builder.RegisterModule(new WarehouseModuleInitializator(_configuration));
        builder.RegisterModule<ApiModule>();
    }
}
