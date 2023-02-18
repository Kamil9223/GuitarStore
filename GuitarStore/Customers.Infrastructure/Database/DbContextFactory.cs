using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Customers.Infrastructure.Database;

internal class DbContextFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public DbContextFactory() { }

    public CustomersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CustomersDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_DB_STRING");

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
        return new CustomersDbContext(optionsBuilder.Options);
    }
}
