using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Orders.Infrastructure.Database;
internal class DbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public DbContextFactory() { }

    public OrdersDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();

        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
        return new OrdersDbContext(optionsBuilder.Options);
    }
}
