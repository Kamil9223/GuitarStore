using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Database;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Catalog.Infrastructure.Configuration;

internal static class InfrastructureModule
{
    internal static void AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<CatalogDbContext>(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;

            var dbOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new CatalogDbContext(dbOptions);
        });

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("QueryService")), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;
            return new SqlConnectionFactory(connectionString);
        });
    }
}
