using Common.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.EndToEnd.Setup.Modules.Common;
using Warehouse.Core.Database;

namespace Tests.EndToEnd.Setup.Modules.Outbox;

internal class OutboxDbSetup : IDbSetup
{
    public void SetupDb(IServiceCollection services, string connectionString)
    {
        var dbOptionsBuilder = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseSqlServer(connectionString)
            .EnableDetailedErrors();

        var context = new OutboxDbContext(dbOptionsBuilder.Options);

        try
        {
            context.Database.Migrate();
        }
        catch
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        services.RemoveAll<OutboxDbContext>();
        services.RemoveAll<DbContextOptions<OutboxDbContext>>();
        services.AddDbContext<OutboxDbContext>(x =>
        {
            x.UseSqlServer(connectionString);
        });
    }
}