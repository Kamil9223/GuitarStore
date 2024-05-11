using Application.Channels;
using Orders.Domain.Orders;
using System.Threading.Channels;

namespace Orders.Application.Orders.BackgroundJobs;

internal sealed record OrderCompletionChannelEvent(Order Order);

internal sealed class OrderCompletionChannel : IChannelPublisher<OrderCompletionChannelEvent>, IChannelReader<OrderCompletionChannelEvent>
{
    private readonly Channel<OrderCompletionChannelEvent> _channel;

    public OrderCompletionChannel()
    {
        var options = new UnboundedChannelOptions
        {
            SingleReader = true
        };

        _channel = Channel.CreateUnbounded<OrderCompletionChannelEvent>(options);
    }

    public ValueTask Publish(OrderCompletionChannelEvent @event, CancellationToken cancellationToken) => _channel.Writer.WriteAsync(@event, cancellationToken);

    public IAsyncEnumerable<OrderCompletionChannelEvent> ReadAll(CancellationToken cancellationToken) => _channel.Reader.ReadAllAsync(cancellationToken);
}
