using Application.RabbitMq.Abstractions;
using Catalog.Application.Products.Events.Outgoing;
using Catalog.Application.Products.Services;
using Catalog.Shared;

namespace Catalog.Application.Products.ModuleApi;
internal class ProductService : IProductService
{
    private readonly IProductQueryService _productQueryService;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public ProductService(IProductQueryService productQueryService, IIntegrationEventPublisher integrationEventPublisher)
    {
        _productQueryService = productQueryService;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task ForceProductsSynchronization()
    {
        var products = await _productQueryService.GetAll();

        var @event = new ProductsSynchronizationForcedEvent(
            products.Select(
                p => new ProductsSynchronizationForcedData(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Quantity))
                .ToList());

        await _integrationEventPublisher.Publish(@event);
    }
}
