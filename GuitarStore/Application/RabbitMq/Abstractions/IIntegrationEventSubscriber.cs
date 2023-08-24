using Application.RabbitMq.Abstractions.Events;
using Infrastructure.RabbitMq;

namespace Application.RabbitMq.Abstractions;

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
