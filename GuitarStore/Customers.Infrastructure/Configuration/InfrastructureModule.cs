using Autofac;
using Customers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Customers.Infrastructure.Configuration;

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
            var dbOptions = new DbContextOptionsBuilder<CustomersDbContext>();
            dbOptions.UseSqlServer(_configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!);
            return new CustomersDbContext(dbOptions.Options);
        })
        .As<CustomersDbContext>()
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
