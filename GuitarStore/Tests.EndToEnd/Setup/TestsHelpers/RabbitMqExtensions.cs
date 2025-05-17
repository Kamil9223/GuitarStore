using Application.RabbitMq.Abstractions.Events;
using Infrastructure.RabbitMq.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Tests.EndToEnd.Setup.TestsHelpers;
internal static class RabbitMqExtensions
{
    public static TaskCompletionSource<bool> CreateTestConsumerForPublishing<TEvent>(this IRabbitMqChannel rabbitMqChannel)
        where TEvent : IntegrationEvent
    {
        var queueName = $"test_events_queue_{Guid.NewGuid()}";
        rabbitMqChannel.Channel.QueueDeclare(queue: queueName, durable: false, exclusive: true, autoDelete: true, arguments: null);
        rabbitMqChannel.Channel.QueueBind(queue: queueName, exchange: "GuitarStore", routingKey: typeof(TEvent).Name, arguments: null);

        var tcs = new TaskCompletionSource<bool>();
        var consumer = new AsyncEventingBasicConsumer(rabbitMqChannel.Channel);
        consumer.Received += (_, ea) =>
        {
            tcs.TrySetResult(true);
            return Task.CompletedTask;
        };
        rabbitMqChannel.Channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        return tcs;
    }

    public static void PublishTestEvent<TEvent>(this IRabbitMqChannel rabbitMqChannel, TEvent @event)
        where TEvent : IntegrationEvent
    {
        var json = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(json);

        rabbitMqChannel.Channel.BasicPublish(
            exchange: "GuitarStore",
            routingKey: typeof(TEvent).Name,
            mandatory: true,
            //basicProperties: properties,
            body: body);
    }
}
