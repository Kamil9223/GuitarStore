using Application.CQRS.Command;
using Common.EfCore.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Core.Commands;
using Warehouse.Core.Database;
using Warehouse.Core.InternalModuleApi;
using Warehouse.Shared;

namespace Warehouse.Core;
public static class WarehouseModuleInitializator
{
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;

            var dbOptions = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new WarehouseDbContext(dbOptions);
        });
        services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());

        services.AddScoped<IProductReservationService, ProductReservationService>();

        services.AddScoped<ICommandHandler<IncreaseStockQuantityCommand>, IncreaseStockQuantityCommandHandler>();
        services.Decorate<ICommandHandler<IncreaseStockQuantityCommand>, DbContextTransactionDecorator<IWarehouseDbContext, IncreaseStockQuantityCommand>>();

        return services;
    }
}
