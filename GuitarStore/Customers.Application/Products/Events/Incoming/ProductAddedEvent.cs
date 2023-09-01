using Application.RabbitMq.Abstractions.Events;

namespace Customers.Application.Products.Events.Incoming;

internal class ProductAddedEvent : IntegrationEvent, IIntegrationConsumeEvent
{
    public string Name { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
}
