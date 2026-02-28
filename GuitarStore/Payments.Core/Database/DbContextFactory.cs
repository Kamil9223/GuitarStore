using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Payments.Core.Database;

internal class DbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public DbContextFactory() { }

    public PaymentsDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PaymentsDbContext>();
        var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value;

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        optionsBuilder.UseSqlServer(
            sqlConnectionStringBuilder.ConnectionString,
            x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, PaymentsDbContext.Schema));
        return new PaymentsDbContext(optionsBuilder.Options);
    }
}