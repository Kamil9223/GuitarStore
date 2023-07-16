using RabbitMQ.Client;

namespace Infrastructure.RabbitMq.Abstractions;

public interface IRabbitMqChannel
{
    IModel Channel { get; }
}
