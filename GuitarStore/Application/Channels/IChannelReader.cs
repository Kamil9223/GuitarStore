namespace Application.Channels;
public interface IChannelReader<ChannelEvent> where ChannelEvent : class
{
    /// <summary>
    /// Read all events from channel using async Enumerable iteration.
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns><see cref="IAsyncEnumerable{ChannelEvent}"/></returns>
    public IAsyncEnumerable<ChannelEvent> ReadAll(CancellationToken cancellationToken);
}
