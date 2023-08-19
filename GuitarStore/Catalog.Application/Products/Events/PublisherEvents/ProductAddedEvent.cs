using Application.RabbitMq.Abstractions.Events;

namespace Catalog.Application.Products.Events.PublisherEvents;

internal class ProductAddedEvent : IntegrationEvent, IIntegrationPublishEvent
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
}
