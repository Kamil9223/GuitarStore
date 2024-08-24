using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Warehouse.Core.Database;
using Warehouse.Core.InternalModuleApi;

namespace Warehouse.Core;
public sealed class WarehouseModuleInitializator : Module
{
    private readonly IConfiguration _configuration;

    public WarehouseModuleInitializator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context =>
        {
            var dbOptions = new DbContextOptionsBuilder<WarehouseDbContext>();
            dbOptions.UseSqlServer(_configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!);
            return new WarehouseDbContext(dbOptions.Options);
        })
       .As<WarehouseDbContext>()
       .InstancePerLifetimeScope();

        builder.RegisterType<ProductReservationService>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}
