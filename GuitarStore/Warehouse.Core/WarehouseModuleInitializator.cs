using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Core.Database;
using Warehouse.Core.InternalModuleApi;
using Warehouse.Shared;

namespace Warehouse.Core;
public static class WarehouseModuleInitializator
{
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<WarehouseDbContext>(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;

            var dbOptions = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new WarehouseDbContext(dbOptions);
        });

        services.AddScoped<IProductReservationService, ProductReservationService>();

        return services;
    }
}
