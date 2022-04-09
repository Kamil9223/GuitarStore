using Autofac;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Warehouse.Infrastructure.Database;
using Module = Autofac.Module;

namespace Warehouse.Infrastructure.Configuration;

internal sealed class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context =>
        {
            var configuration = context.Resolve<IConfiguration>();
            var dbOptions = new DbContextOptionsBuilder<WarehouseDbContext>();
            dbOptions.UseSqlServer(configuration.GetSection("ConnectionStrings:Warehouse").Value);
            return new WarehouseDbContext(dbOptions.Options);
        })
            .As<DbContext>()
            .InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => type.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(GenericRepository<>))
            .As(typeof(IRepository<>))
            .InstancePerLifetimeScope();
    }
}
