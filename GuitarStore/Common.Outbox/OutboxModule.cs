using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Outbox;

public static class OutboxModule
{
    public static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration, bool skipHostedServices = false)
    {
        services.AddScoped<OutboxDbContext>(provider =>
        {
            var connectionString = configuration.GetRequiredSection("ConnectionStrings:GuitarStore").Value!;

            var dbOptions = new DbContextOptionsBuilder<OutboxDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new OutboxDbContext(dbOptions);
        });
        services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();
        if (!skipHostedServices)
            services.AddHostedService<OutboxMessageDispatcherJob>();
        return services;
    }
}