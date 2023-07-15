using Application.RabbitMq.Abstractions.Events;
using Infrastructure.RabbitMq;

namespace Application.RabbitMq.Abstractions;

/// <summary>
/// Application event subscriber
/// </summary>
public interface IAppEventSubscriber
{
    /// <summary>
    /// Subscribe to queue
    /// </summary>
    void Subscribe<TEvent>(TEvent @event, RabbitMqQueueName queueName) where TEvent : ApplicationEvent;
}
