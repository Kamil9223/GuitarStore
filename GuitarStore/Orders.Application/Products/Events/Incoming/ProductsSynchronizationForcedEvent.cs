using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;

namespace Orders.Application.Products.Events.Incoming;
internal sealed record ProductsSynchronizationForcedEvent(IReadOnlyCollection<ProductsSynchronizationForcedData> Products)
    : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed record ProductsSynchronizationForcedData(int Id, string Name, decimal Price, int Quantity);

internal sealed class ProductsSynchronizationForcedEventHandler : IIntegrationEventHandler<ProductsSynchronizationForcedEvent>
{
    public async Task Handle(ProductsSynchronizationForcedEvent @event)
    {
        //sync products, new add, same ignore
    }
}
