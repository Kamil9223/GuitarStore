using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.EndToEnd.Setup.Modules.Common;
using Warehouse.Core.Database;

namespace Tests.EndToEnd.Setup.Modules.Warehouse;
internal class WarehouseDbSetup : IDbSetup
{
    public void SetupDb(IServiceCollection services, string connectionString)
    {
        var dbOptionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>()
          .UseSqlServer(connectionString)
          .EnableDetailedErrors();

        var context = new WarehouseDbContext(dbOptionsBuilder.Options);

        try
        {
            context.Database.Migrate();
        }
        catch
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        services.RemoveAll<WarehouseDbContext>();
        services.RemoveAll<DbContextOptions<WarehouseDbContext>>();
        services.AddDbContext<WarehouseDbContext>(x =>
        {
            x.UseSqlServer(connectionString);
        });
    }
}
