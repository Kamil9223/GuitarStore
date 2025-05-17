using Common.EfCore.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Abstractions;
using Orders.Infrastructure.Database;
using System.Reflection;

namespace Orders.Infrastructure.Configuration;

internal static class InfrastructureModule
{
    internal static void AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<OrdersDbContext>(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;

            var dbOptions = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new OrdersDbContext(dbOptions);
        });

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
    }
}
