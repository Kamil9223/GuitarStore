using Common.RabbitMq.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.RabbitMq;

internal class RabbitMqSubscriptionBackgroundService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RabbitMqSubscriptionBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var subscriptionManagers = scope.ServiceProvider.GetRequiredService<IEnumerable<IEventBusSubscriptionManager>>();
        foreach (var sub in subscriptionManagers)
            sub.SubscribeToEvents();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
