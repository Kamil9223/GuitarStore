using Customers.Application.Abstractions;
using Customers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Customers.Infrastructure.Configuration;

internal static class InfrastructureModule
{
    internal static void AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<CustomersDbContext>(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;

            var dbOptions = new DbContextOptionsBuilder<CustomersDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new CustomersDbContext(dbOptions);
        });

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
