using RabbitMQ.Client;

namespace Common.RabbitMq.Abstractions;

public interface IRabbitMqConnector : IDisposable
{
    bool IsConnected { get; }

    void Connect();

    IModel CreateChannel();
}
