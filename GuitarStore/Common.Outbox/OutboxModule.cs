using Microsoft.Extensions.DependencyInjection;

namespace Common.Outbox;

public static class OutboxModule
{
    public static IServiceCollection AddOutbox(this IServiceCollection services, bool skipHostedServices = false)
    {
        if (!skipHostedServices)
            services.AddHostedService<OutboxMessageDispatcherJob>();
        return services;
    }
}
