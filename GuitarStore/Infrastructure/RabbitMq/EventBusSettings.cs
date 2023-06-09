using Infrastructure.RabbitMq.Abstractions;

namespace Infrastructure.RabbitMq;

internal class EventBusSettings : IEventBusSettings
{
    public string ExchangeName { get; init; } = null!;

    public IEnumerable<string> QueuesNames { get; init; } = null!;
}
