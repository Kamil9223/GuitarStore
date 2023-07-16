using Autofac;
using Catalog.Application;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infrastructure.Configuration;

public sealed class CatalogModuleInitializator : Module
{
    private readonly IConfiguration _configuration;

    public CatalogModuleInitializator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule(new InfrastructureModule(_configuration));
        builder.RegisterModule<ApplicationModule>();
    }
}
