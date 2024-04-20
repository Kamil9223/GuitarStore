using Autofac;
using Catalog.Infrastructure.Configuration;
using Customers.Infrastructure.Configuration;
using Infrastructure.Configuration;
using Orders.Infrastructure.Configuration;

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
        builder.RegisterModule<ApiModule>();
    }
}
