using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infrastructure.Database;

internal class DbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public DbContextFactory() { }

    public CatalogDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        //var connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_DB_STRING");
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
        return new CatalogDbContext(optionsBuilder.Options);
    }
}
