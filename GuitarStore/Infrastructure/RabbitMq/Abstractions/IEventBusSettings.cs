namespace Infrastructure.RabbitMq.Abstractions;

public interface IEventBusSettings
{
    string ExchangeName { get; init; }

    IEnumerable<string> QueuesNames { get; init; }
}
