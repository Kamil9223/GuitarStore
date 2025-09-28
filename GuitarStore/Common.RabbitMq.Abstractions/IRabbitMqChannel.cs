using RabbitMQ.Client;

namespace Common.RabbitMq.Abstractions;

public interface IRabbitMqChannel
{
    IModel Channel { get; }
}
