using Application.RabbitMq.Abstractions.Events;

namespace Catalog.Application.Products.Events.Outgoing;

internal sealed record ProductsSynchronizationForcedEvent(IReadOnlyCollection<ProductsSynchronizationForcedData> Products)
    : IntegrationEvent, IIntegrationPublishEvent;

internal sealed record ProductsSynchronizationForcedData(int Id, string Name, decimal Price, int Quantity);
