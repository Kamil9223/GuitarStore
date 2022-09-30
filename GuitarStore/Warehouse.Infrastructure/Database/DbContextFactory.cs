using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Warehouse.Infrastructure.Database;

internal class DbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public DbContextFactory() { }

    public WarehouseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_DB_STRING");

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
        return new WarehouseDbContext(optionsBuilder.Options);
    }
}
