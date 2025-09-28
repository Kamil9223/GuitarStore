using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Customers.Application.Products.Events.Incoming;

namespace Customers.Application;

internal class EventBusSubscriptionManager : IEventBusSubscriptionManager
{
    private readonly IIntegrationEventSubscriber _integrationEventSubscriber;

    public EventBusSubscriptionManager(IIntegrationEventSubscriber integrationEventSubscriber)
    {
        _integrationEventSubscriber = integrationEventSubscriber;
    }

    public void SubscribeToEvents()
    {
        _integrationEventSubscriber.Subscribe<ProductAddedEvent, ProductAddedEventHandler>(RabbitMqQueueName.CustomersQueue);
    }
}
