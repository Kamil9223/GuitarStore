using Autofac;
using Microsoft.Extensions.Configuration;
using Orders.Application;

namespace Orders.Infrastructure.Configuration;
public sealed class OrdersModuleInitializator : Module
{
    private readonly IConfiguration _configuration;

    public OrdersModuleInitializator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule(new InfrastructureModule(_configuration));
        builder.RegisterModule<ApplicationModule>();
    }
}
