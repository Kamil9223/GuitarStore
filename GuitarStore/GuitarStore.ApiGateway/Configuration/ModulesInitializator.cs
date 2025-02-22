using Autofac;
using Catalog.Infrastructure.Configuration;
using Customers.Infrastructure.Configuration;
using Infrastructure.Configuration;
using Orders.Infrastructure.Configuration;
using Payments.Core;
using Warehouse.Core;

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
        builder.RegisterModule(new CatalogModuleInitializator(_configuration));
        builder.RegisterModule(new CustomersModuleInitializator(_configuration));
        builder.RegisterModule(new OrdersModuleInitializator(_configuration));
        builder.RegisterModule(new WarehouseModuleInitializator(_configuration));
        builder.RegisterModule(new PaymentsModuleInitializator());
        builder.RegisterModule<ApiModule>();
    }
}
