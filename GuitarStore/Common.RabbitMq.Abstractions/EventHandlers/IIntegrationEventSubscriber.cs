using Common.RabbitMq.Abstractions.Events;

namespace Common.RabbitMq.Abstractions.EventHandlers;

/// <summary>
/// Application event subscriber
/// </summary>
public interface IIntegrationEventSubscriber
{
    /// <summary>
    /// Subscribe to queue
    /// </summary>
    void Subscribe<TEvent, TEventHandler>(RabbitMqQueueName queueName)
        where TEvent : IntegrationEvent, IIntegrationConsumeEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>;
}
