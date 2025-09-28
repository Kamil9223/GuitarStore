using Common.RabbitMq.Abstractions;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Common.RabbitMq;

internal class RabbitMqConnector : IRabbitMqConnector, IRabbitMqChannel
{
    private readonly IConfiguration _configuration;

    private IConnection _connection = null!;
    public bool Disposed;

    public RabbitMqConnector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsConnected => _connection is not null && _connection is { IsOpen: true } && !Disposed;

    public IModel Channel { get; private set; } = null!;

    public IModel CreateChannel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        Channel = _connection.CreateModel();
        return Channel;
    }

    public void Connect()
    {
        var connectionString = _configuration.GetRequiredSection("ConnectionStrings:RabbitMq").Value;
        if (connectionString is null)
        {
            throw new ArgumentNullException("RabbitMQ connection string is null.");
        }

        var connectionFactory = new ConnectionFactory();
        connectionFactory.Uri = new Uri(connectionString);
        connectionFactory.AutomaticRecoveryEnabled = true;
        connectionFactory.DispatchConsumersAsync = true;

        _connection = connectionFactory.CreateConnection();
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        Disposed = true;

        try
        {
            _connection.Dispose();
        }
        catch (Exception ex)
        {
            //_logger.LogCritical(ex.ToString());
        }
    }
}
