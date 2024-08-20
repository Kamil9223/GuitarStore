using Application.Channels;
using Autofac;
using Microsoft.Extensions.Hosting;
using Orders.Domain.Orders;
using Orders.Domain.Products;

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
        await foreach (var @event in _orderCompletionChannelReader.ReadAll(stoppingToken))
        {
            try
            {
                await HandleEvent(@event);
            }
            catch (Exception ex)
            {
                //log Error
            }
        }
    }

    private async Task HandleEvent(OrderCompletionChannelEvent @event)
    {
        using var scope = _scope.BeginLifetimeScope();
        var productRepository = scope.Resolve<IProductRepository>();
        var newOrder = @event.Order;

        //do zastanowienia: być może trzeba całą obsługę eventu dać w jakimś Task.Runie, żeby nie blokować pracy joba w przypadku wieeelu zamwówień
        //przemyśleć tutaj kwestię bezpieczeństwa wątków



        //jaka forma płatności
    }
}
