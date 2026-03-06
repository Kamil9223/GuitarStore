using Common.RabbitMq.Abstractions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Warehouse.Core.Events.Incoming;

namespace Warehouse.Core.Events;

internal sealed class EventBusSubscriptionManager : IEventBusSubscriptionManager
{
    private readonly IIntegrationEventSubscriber _integrationEventSubscriber;

    public EventBusSubscriptionManager(IIntegrationEventSubscriber integrationEventSubscriber)
    {
        _integrationEventSubscriber = integrationEventSubscriber;
    }

    public void SubscribeToEvents()
    {
        _integrationEventSubscriber.Subscribe<OrderPaidEvent, OrderPaidEventHandler>(RabbitMqQueueName.WarehouseQueue);
        _integrationEventSubscriber.Subscribe<OrderCancelledEvent, OrderCancelledEventHandler>(RabbitMqQueueName.WarehouseQueue);
        _integrationEventSubscriber.Subscribe<OrderExpiredEvent, OrderExpiredEventHandler>(RabbitMqQueueName.WarehouseQueue);
    }
}
