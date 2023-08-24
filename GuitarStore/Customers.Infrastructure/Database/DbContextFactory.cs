using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Customers.Infrastructure.Database;

internal class DbContextFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public DbContextFactory() { }

    public CustomersDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CustomersDbContext>();
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
        return new CustomersDbContext(optionsBuilder.Options);
    }
}
