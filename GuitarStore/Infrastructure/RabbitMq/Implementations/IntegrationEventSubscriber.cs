using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Autofac;
using Infrastructure.RabbitMq.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Infrastructure.RabbitMq.Implementations;

internal class IntegrationEventSubscriber : IIntegrationEventSubscriber
{
    private readonly IRabbitMqChannel _rabbitMqChannel;
    private readonly ILifetimeScope _scope;
    private readonly IntegrationEventsSubscriptionManager _integrationEventsSubscriptionManager;

    public IntegrationEventSubscriber(IRabbitMqChannel rabbitMqChannel, ILifetimeScope scope, IntegrationEventsSubscriptionManager integrationEventsSubscriptionManager)
    {
        _rabbitMqChannel = rabbitMqChannel;
        _scope = scope;
        _integrationEventsSubscriptionManager = integrationEventsSubscriptionManager;
    }

    public void Subscribe<TEvent, TEventHandler>(RabbitMqQueueName queueName)
        where TEvent : IntegrationEvent, IIntegrationConsumeEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        _integrationEventsSubscriptionManager.AddSubscription<TEvent, TEventHandler>();

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

                using var scope = _scope.BeginLifetimeScope();
                var handlerType = _integrationEventsSubscriptionManager.GetHandlerTypeForEvent(typeof(TEvent));

                var handler = scope.Resolve(handlerType);
                var typedHandler = handler as IIntegrationEventHandler<TEvent>;
                await typedHandler.Handle(integrationEvent);
            }
            catch (Exception ex)
            {
                //logg exception
            }

            _rabbitMqChannel.Channel.BasicAck(@event.DeliveryTag, multiple: false);
        }
    }
}
