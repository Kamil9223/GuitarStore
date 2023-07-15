using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Infrastructure.RabbitMq.Abstractions;
using Newtonsoft.Json;
using System.Text;

namespace Infrastructure.RabbitMq.Implementations;

internal class AppEventPublisher : IAppEventPublisher
{
    private readonly IRabbitMqChannel _rabbitMqChannel;

    public AppEventPublisher(IRabbitMqChannel rabbitMqChannel)
    {
        _rabbitMqChannel = rabbitMqChannel;
    }

    public Task Publish<TEvent>(TEvent @event) where TEvent : ApplicationEvent, IPublishEvent
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
