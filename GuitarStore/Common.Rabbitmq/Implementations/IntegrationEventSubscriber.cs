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

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += Consumer_Received;

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping);
            if (_handlerTimeout > TimeSpan.Zero)
                linkedCts.CancelAfter(_handlerTimeout);
            var ct = linkedCts.Token;

            try
            {
                var message = Encoding.UTF8.GetString(@event.Body.Span);
                var integrationEvent = JsonConvert.DeserializeObject<TEvent>(message);

                using var scope = _serviceScopeFactory.CreateScope();
                var handlerAbstractionType = typeof(IIntegrationEventHandler<>).MakeGenericType(typeof(TEvent));

                var handler = scope.ServiceProvider.GetRequiredService(handlerAbstractionType);

                var typedHandler = handler as IIntegrationEventHandler<TEvent>;
                await typedHandler!.Handle(integrationEvent!, ct);
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
