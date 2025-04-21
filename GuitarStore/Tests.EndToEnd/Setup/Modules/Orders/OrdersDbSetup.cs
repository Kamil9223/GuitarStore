using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orders.Infrastructure.Database;
using Tests.EndToEnd.Setup.Modules.Common;

namespace Tests.EndToEnd.Setup.Modules.Orders;
internal class OrdersDbSetup : IDbSetup
{
    public void SetupDb(IServiceCollection services, string connectionString)
    {
        var dbOptionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>()
          .UseSqlServer(connectionString)
          .EnableDetailedErrors();

        var context = new OrdersDbContext(dbOptionsBuilder.Options);

        try
        {
            context.Database.Migrate();
        }
        catch
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        services.RemoveAll<OrdersDbContext>();
        services.AddDbContextFactory<OrdersDbContext>(x =>
        {
            x.UseSqlServer(connectionString);
        });
    }
}
