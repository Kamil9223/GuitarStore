using Application.RabbitMq;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.RabbitMq;

internal class RabbitMqSubscriptionBackgroundService : IHostedService
{
    private readonly ILifetimeScope _scope;

    public RabbitMqSubscriptionBackgroundService(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scope.BeginLifetimeScope();
        var subscriptionManagers = scope.Resolve<IEnumerable<IEventBusSubscriptionManager>>();
        foreach (var sub in subscriptionManagers)
            sub.SubscribeToEvents();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
