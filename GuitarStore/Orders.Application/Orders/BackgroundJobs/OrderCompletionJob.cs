using Application.Channels;
using Autofac;
using Catalog.Shared;
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

        var productTasks = new List<Task<Product>>();
        foreach (var orderItem in newOrder.OrderItems)
            productTasks.Add(productRepository.Get(orderItem.ProductId));

        await Task.WhenAll(productTasks);

        if (productTasks.Any(task => task.IsFaulted))
        {
            //zalogować to info
            //wymusic synchronizację produktów, a jesli to się powtorzy po synchronizacji to... przemyslec

            //co jeśli 1000 zamówień poprosi o synchronizację produktów ?
            //Może jakiś mechanizm, że synchronizacja może być wykonana jedynie raz na godzinę ?
            //persystencja evenu proszącego o synchronizację, i potem job który zaczytuje sobie to info i robi raz na godzinę (jeśli jest event) synch

            //dodanie do nowej tabelki (nie klasa domenowa, a infrastrukturalna) o oczekującą intencję o synchronizacji.
            //Job będzie co 15minut sprawdzał tą tabelkę i wykonywał (po statusie patrząc) jeśli jeszcze nie wykonana
            //a
            //tutaj natomiast trzeba jedynie dodać wpis do tabelki ? To musi być idempotetne zachowanie, jeśli już taki jest to ignoruj, nie dodawaj
        }

        //jaka forma płatności
    }

    private async Task CheckProductsOnStock(Order order)
    {

    }
}
