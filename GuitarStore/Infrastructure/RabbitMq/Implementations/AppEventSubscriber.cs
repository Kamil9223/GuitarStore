using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Infrastructure.RabbitMq.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.RabbitMq.Implementations;

internal class AppEventSubscriber : IAppEventSubscriber
{
    private readonly IRabbitMqChannel _rabbitMqChannel;

    public AppEventSubscriber(IRabbitMqChannel rabbitMqChannel)
    {
        _rabbitMqChannel = rabbitMqChannel;
    }

    public void Subscribe<TEvent>(TEvent @event, RabbitMqQueueName queueName) where TEvent : ApplicationEvent
    {
        var channel = _rabbitMqChannel.Channel;

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += Consumer_Received;

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
    }

    private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {
        throw new NotImplementedException();
    }
}
