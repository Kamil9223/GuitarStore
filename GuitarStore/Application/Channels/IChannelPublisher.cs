namespace Application.Channels;
public interface IChannelPublisher<ChannelEvent> where ChannelEvent : class
{
    /// <summary>
    /// Publish event to channel.
    /// </summary>
    /// <param name="event">Event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public ValueTask Publish(ChannelEvent @event, CancellationToken cancellationToken);
}
