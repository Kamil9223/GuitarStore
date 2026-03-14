using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Auth.Core.Data;

public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(
            sqlConnectionStringBuilder.ConnectionString,
            x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, AuthDbContext.Schema));
        return new AuthDbContext(optionsBuilder.Options);
    }
}
