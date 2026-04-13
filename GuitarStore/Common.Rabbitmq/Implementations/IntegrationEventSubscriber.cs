using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Common.RabbitMq.Implementations;

internal class IntegrationEventSubscriber : IIntegrationEventSubscriber
{
    private readonly IRabbitMqChannel _rabbitMqChannel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly object _subscriptionsLock = new();
    private readonly Dictionary<string, Dictionary<string, SubscriptionRegistration>> _subscriptionsByQueue = new(StringComparer.Ordinal);
    private readonly HashSet<string> _startedConsumers = new(StringComparer.Ordinal);

    private readonly TimeSpan _handlerTimeout = TimeSpan.Zero;//TODO: pobrać timeout w settings

    public IntegrationEventSubscriber(
        IRabbitMqChannel rabbitMqChannel,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _rabbitMqChannel = rabbitMqChannel;
        _serviceScopeFactory = serviceScopeFactory;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public void Subscribe<TEvent, TEventHandler>(RabbitMqQueueName queueName)
        where TEvent : IntegrationEvent, IIntegrationConsumeEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var channel = _rabbitMqChannel.Channel;
        var queue = queueName.QueueName;
        var routingKey = typeof(TEvent).Name;

        lock (_subscriptionsLock)
        {
            if (!_subscriptionsByQueue.TryGetValue(queue, out var subscriptions))
            {
                subscriptions = new Dictionary<string, SubscriptionRegistration>(StringComparer.Ordinal);
                _subscriptionsByQueue[queue] = subscriptions;
            }

            subscriptions[routingKey] = new SubscriptionRegistration(
                typeof(TEvent),
                typeof(IIntegrationEventHandler<TEvent>));

            if (_startedConsumers.Add(queue))
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += ConsumerReceived;

                channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);
            }
        }

        async Task ConsumerReceived(object sender, BasicDeliverEventArgs @event)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping);
            if (_handlerTimeout > TimeSpan.Zero)
            {
                linkedCts.CancelAfter(_handlerTimeout);
            }

            var ct = linkedCts.Token;
            SubscriptionRegistration? registration = null;

            lock (_subscriptionsLock)
            {
                _subscriptionsByQueue.TryGetValue(queue, out var queueSubscriptions);
                queueSubscriptions?.TryGetValue(@event.RoutingKey, out registration);
            }

            if (registration is null)
            {
                _rabbitMqChannel.Channel.BasicNack(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            try
            {
                var message = Encoding.UTF8.GetString(@event.Body.Span);
                var integrationEvent = JsonConvert.DeserializeObject(message, registration.EventType)
                    ?? throw new InvalidOperationException($"RabbitMQ message for routing key '{@event.RoutingKey}' could not be deserialized.");

                using var scope = _serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService(registration.HandlerInterfaceType);

                await registration.InvokeAsync(handler, integrationEvent, ct);
                _rabbitMqChannel.Channel.BasicAck(@event.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _rabbitMqChannel.Channel.BasicNack(@event.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }

    private sealed class SubscriptionRegistration
    {
        public Type EventType { get; }
        public Type HandlerInterfaceType { get; }

        public SubscriptionRegistration(Type eventType, Type handlerInterfaceType)
        {
            EventType = eventType;
            HandlerInterfaceType = handlerInterfaceType;
        }

        public Task InvokeAsync(object handler, object integrationEvent, CancellationToken cancellationToken)
        {
            var handleMethod = HandlerInterfaceType.GetMethod("Handle")
                ?? throw new InvalidOperationException($"Handle method was not found for '{HandlerInterfaceType.Name}'.");

            return (Task)(handleMethod.Invoke(handler, [integrationEvent, cancellationToken])
                ?? throw new InvalidOperationException($"Handler '{HandlerInterfaceType.Name}' returned null Task."));
        }
    }
}
