using Application.RabbitMq;
using Application.RabbitMq.Abstractions;
using Infrastructure.RabbitMq;
using Payments.Core.Events.Incoming;

namespace Payments.Core.Events;
internal class EventBusSubscriptionManager : IEventBusSubscriptionManager
{
    private readonly IIntegrationEventSubscriber _integrationEventSubscriber;

    public EventBusSubscriptionManager(IIntegrationEventSubscriber integrationEventSubscriber)
    {
        _integrationEventSubscriber = integrationEventSubscriber;
    }

    public void SubscribeToEvents()
    {
        _integrationEventSubscriber.Subscribe<CreatedOrderEvent, CreatedOrderEventHandler>(RabbitMqQueueName.PaymentsQueue);
    }
}
