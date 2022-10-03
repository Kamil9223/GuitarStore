using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Warehouse.Application.DataAccessAbstraction;
using Warehouse.Infrastructure.Database;
using Module = Autofac.Module;

namespace Warehouse.Infrastructure.Configuration;

internal sealed class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        IConfiguration? configuration = null;

        builder.Register(context =>
        {
            configuration = context.Resolve<IConfiguration>();
            var dbOptions = new DbContextOptionsBuilder<WarehouseDbContext>();
            dbOptions.UseSqlServer(configuration.GetSection("ConnectionStrings:Warehouse").Value);
            return new WarehouseDbContext(dbOptions.Options);
        })
            .As<DbContext>()
            .InstancePerLifetimeScope();

        builder.RegisterType<SqlConnectionFactory>()
                .As<ISqlConnectionFactory>()
                .WithParameter("connectionString", configuration.GetSection("ConnectionStrings:Warehouse").Value)
                .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => type.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
