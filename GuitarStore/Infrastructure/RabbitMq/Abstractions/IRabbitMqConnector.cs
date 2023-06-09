using RabbitMQ.Client;

namespace Infrastructure.RabbitMq.Abstractions;

internal interface IRabbitMqConnector : IDisposable
{
    bool IsConnected { get; }

    void Connect();

    IModel CreateChannel();
}
