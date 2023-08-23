using Autofac;
using Customers.Application;
using Microsoft.Extensions.Configuration;

namespace Customers.Infrastructure.Configuration;

public sealed class CustomersModuleInitializator : Module
{
    private readonly IConfiguration _configuration;

    public CustomersModuleInitializator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule(new InfrastructureModule(_configuration));
        builder.RegisterModule<ApplicationModule>();
    }
}
