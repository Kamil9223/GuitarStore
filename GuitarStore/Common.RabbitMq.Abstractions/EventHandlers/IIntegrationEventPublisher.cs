using Common.RabbitMq.Abstractions.Events;

namespace Common.RabbitMq.Abstractions.EventHandlers;

/// <summary>
/// Application Event Publisher
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publish Event
    /// </summary>
    /// <param name="event"></param>
    Task Publish<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IntegrationEvent, IIntegrationPublishEvent;
}
