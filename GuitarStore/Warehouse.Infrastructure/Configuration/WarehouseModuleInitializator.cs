using Autofac;
using Microsoft.Extensions.Configuration;
using Warehouse.Application;

namespace Warehouse.Infrastructure.Configuration;

public sealed class WarehouseModuleInitializator : Module
{
    private readonly IConfiguration _configuration;

    public WarehouseModuleInitializator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule(new InfrastructureModule(_configuration));
        builder.RegisterModule<ApplicationModule>();
    }
}
