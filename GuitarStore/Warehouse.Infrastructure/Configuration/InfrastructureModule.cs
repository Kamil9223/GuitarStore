using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Warehouse.Application.Abstractions;
using Warehouse.Infrastructure.Database;
using Module = Autofac.Module;

namespace Warehouse.Infrastructure.Configuration;

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
            var dbOptions = new DbContextOptionsBuilder<WarehouseDbContext>();
            dbOptions.UseSqlServer(_configuration.GetSection("ConnectionStrings:GuitarStore").Value);
            return new WarehouseDbContext(dbOptions.Options);
        })
            .As<DbContext>()
            .InstancePerLifetimeScope();

        builder.RegisterType<SqlConnectionFactory>()
                .As<ISqlConnectionFactory>()
                .WithParameter("connectionString", _configuration.GetSection("ConnectionStrings:GuitarStore").Value)
                .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => type.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
