using Customers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.EndToEnd.Setup.Modules.Common;

namespace Tests.EndToEnd.Setup.Modules.Customers;
internal class CustomersDbSetup : IDbSetup
{
    public void SetupDb(IServiceCollection services, string connectionString)
    {
        var dbOptionsBuilder = new DbContextOptionsBuilder<CustomersDbContext>()
          .UseSqlServer(connectionString)
          .EnableDetailedErrors();

        var context = new CustomersDbContext(dbOptionsBuilder.Options);

        try
        {
            context.Database.Migrate();
        }
        catch
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        services.RemoveAll<CustomersDbContext>();
        services.AddDbContextFactory<CustomersDbContext>(x =>
        {
            x.UseSqlServer(connectionString);
        });
    }
}
