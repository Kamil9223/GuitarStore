using Catalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tests.EndToEnd.Setup.Modules.Catalog;
internal class CatalogDbSetup : IDbSetup
{
    public void SetupDb(IServiceCollection services, string connectionString)
    {
        var dbOptionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>()
          .UseSqlServer(connectionString)
          .EnableDetailedErrors();

        var context = new CatalogDbContext(dbOptionsBuilder.Options);

        try
        {
            context.Database.Migrate();
        }
        catch
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        services.RemoveAll<CatalogDbContext>();
        services.AddDbContextFactory<CatalogDbContext>(x =>
        {
            x.UseSqlServer(connectionString);
        });
    }
}
