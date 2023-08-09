using Autofac;
using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Database;
using Catalog.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Module = Autofac.Module;

namespace Catalog.Infrastructure.Configuration;

internal sealed class InfrastructureModule : Module
{
    private readonly IConfiguration _configuration;

    public InfrastructureModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context =>
        {
            var dbOptions = new DbContextOptionsBuilder<CatalogDbContext>();
            dbOptions.UseSqlServer(_configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value);
            return new CatalogDbContext(dbOptions.Options);
        })
            .As<DbContext>()
            .InstancePerLifetimeScope();

        builder.RegisterType<SqlConnectionFactory>()
                .As<ISqlConnectionFactory>()
                .WithParameter("connectionString", _configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value)
                .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => type.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
