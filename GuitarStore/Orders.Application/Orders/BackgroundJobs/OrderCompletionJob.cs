using Application.Channels;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Orders.Application.Orders.BackgroundJobs;
internal sealed class OrderCompletionJob : BackgroundService
{
    private readonly ILifetimeScope _scope;
    private readonly IChannelReader<OrderCompletionChannelEvent> _orderCompletionChannelReader;

    public OrderCompletionJob(ILifetimeScope scope, IChannelReader<OrderCompletionChannelEvent> orderCompletionChannelReader)
    {
        _scope = scope;
        _orderCompletionChannelReader = orderCompletionChannelReader;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var @event in _orderCompletionChannelReader.ReadAll(stoppingToken))
            {
                var newOrder = @event.Order;
                //sprawdzenie czy wszystko jest na stanie
                //jaka forma płatności
            }
        }
        catch (Exception ex)
        {

        }
    }
}
