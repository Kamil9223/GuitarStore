using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Common.Outbox;

internal class DbContextFactory : IDesignTimeDbContextFactory<OutboxDbContext>
{
    public DbContextFactory() { }

    public OutboxDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<OutboxDbContext>();
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(
            sqlConnectionStringBuilder.ConnectionString,
            x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "outbox"));
        return new OutboxDbContext(optionsBuilder.Options);
    }
}