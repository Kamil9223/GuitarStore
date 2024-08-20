using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Warehouse.Core.Database;
internal class DbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public DbContextFactory() { }

    public WarehouseDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();

        var optionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>();
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
        return new WarehouseDbContext(optionsBuilder.Options);
    }
}
