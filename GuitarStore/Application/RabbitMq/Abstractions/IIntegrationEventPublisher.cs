using Application.RabbitMq.Abstractions.Events;

namespace Application.RabbitMq.Abstractions;

/// <summary>
/// Application Event Publisher
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publish Event
    /// </summary>
    /// <param name="event"></param>
    Task Publish<TEvent>(TEvent @event) where TEvent : IntegrationEvent, IIntegrationPublishEvent;
}
