using Application.RabbitMq;
using Application.RabbitMq.Abstractions;
using Customers.Application.Products.Handlers.EventHandlers;
using Customers.Application.Products.Messages.Events.Incoming;
using Infrastructure.RabbitMq;

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
