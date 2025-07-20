using Application.RabbitMq;
using Application.RabbitMq.Abstractions;
using Infrastructure.RabbitMq;
using Orders.Application.Orders.Events.Incoming;

namespace Orders.Application;
internal class EventBusSubscriptionManager : IEventBusSubscriptionManager
{
    private readonly IIntegrationEventSubscriber _integrationEventSubscriber;

    public EventBusSubscriptionManager(IIntegrationEventSubscriber integrationEventSubscriber)
    {
        _integrationEventSubscriber = integrationEventSubscriber;
    }

    public void SubscribeToEvents()
    {
        _integrationEventSubscriber.Subscribe<OrderPaidEvent, OrderPaidEventHandler>(RabbitMqQueueName.OrdersQueue);
    }
}
