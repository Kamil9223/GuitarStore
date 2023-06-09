namespace Infrastructure.RabbitMq.Abstractions;

/// <summary>
/// Application Event Publisher
/// </summary>
public interface IAppEventPublisher
{
    /// <summary>
    /// Publish Event
    /// </summary>
    /// <param name="event"></param>
    Task Publish<TEvent>(TEvent @event) where TEvent : ApplicationEvent;
}
