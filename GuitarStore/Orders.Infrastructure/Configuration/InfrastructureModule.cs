using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Orders.Infrastructure.Database;
using System.Reflection;

namespace Orders.Infrastructure.Configuration;
internal sealed class InfrastructureModule : Autofac.Module
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
            var dbOptions = new DbContextOptionsBuilder<OrdersDbContext>();
            dbOptions.UseSqlServer(_configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!);
            return new OrdersDbContext(dbOptions.Options);
        })
        .As<OrdersDbContext>()
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
