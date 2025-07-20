using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Infrastructure.RabbitMq.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Infrastructure.RabbitMq.Implementations;

internal class IntegrationEventSubscriber : IIntegrationEventSubscriber
{
    private readonly IRabbitMqChannel _rabbitMqChannel;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IntegrationEventSubscriber(IRabbitMqChannel rabbitMqChannel, IServiceScopeFactory serviceScopeFactory)
    {
        _rabbitMqChannel = rabbitMqChannel;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void Subscribe<TEvent, TEventHandler>(RabbitMqQueueName queueName)
        where TEvent : IntegrationEvent, IIntegrationConsumeEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var channel = _rabbitMqChannel.Channel;

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += Consumer_Received;

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var message = Encoding.UTF8.GetString(@event.Body.Span);
                var integrationEvent = JsonConvert.DeserializeObject<TEvent>(message);

                using var scope = _serviceScopeFactory.CreateScope();
                var handlerAbstractionType = typeof(IIntegrationEventHandler<>).MakeGenericType(typeof(TEvent));

                var handler = scope.ServiceProvider.GetRequiredService(handlerAbstractionType);
                
                var typedHandler = handler as IIntegrationEventHandler<TEvent>;
                await typedHandler!.Handle(integrationEvent!);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //logg exception
            }

            _rabbitMqChannel.Channel.BasicAck(@event.DeliveryTag, multiple: false);
        }
    }
}
