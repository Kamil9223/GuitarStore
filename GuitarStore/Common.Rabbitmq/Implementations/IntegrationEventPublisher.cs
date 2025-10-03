using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Abstractions.Events;
using Newtonsoft.Json;
using System.Text;

namespace Common.RabbitMq.Implementations;

internal class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IRabbitMqChannel _rabbitMqChannel;

    public IntegrationEventPublisher(IRabbitMqChannel rabbitMqChannel)
    {
        _rabbitMqChannel = rabbitMqChannel;
    }

    public Task Publish<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IntegrationEvent, IIntegrationPublishEvent
    {
        var channel = _rabbitMqChannel.Channel;

        var properties = channel.CreateBasicProperties();
        properties.DeliveryMode = 2;
        properties.CorrelationId = @event.CorrelationId.ToString();

        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);
        var eventName = @event.GetType().Name;

        channel.BasicPublish(
               exchange: RabbitMqSetupBackgroundService.ExchangeName,
               routingKey: eventName,
               mandatory: true,
               basicProperties: properties,
               body: body);

        return Task.FromResult(true);
    }
}
