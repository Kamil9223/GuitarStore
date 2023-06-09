namespace Infrastructure.RabbitMq.Abstractions;

/// <summary>
/// Application event subscriber
/// </summary>
public interface IAppEventSubscriber
{
    /// <summary>
    /// Subscribe to queue
    /// </summary>
    /// <param name="event"></param>
    Task Subscribe<TEvent>(TEvent @event);
}
