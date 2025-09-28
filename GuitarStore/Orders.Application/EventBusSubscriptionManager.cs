using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
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
