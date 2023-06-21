using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infrastructure.Database;

internal class DbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public DbContextFactory() { }

    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_DB_STRING");

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
        return new CatalogDbContext(optionsBuilder.Options);
    }
}
