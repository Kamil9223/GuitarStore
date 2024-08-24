using Application.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Catalog.Application.Products.Events.Outgoing;

internal sealed record ProductsSynchronizationForcedEvent(IReadOnlyCollection<ProductsSynchronizationForcedData> Products)
    : IntegrationEvent, IIntegrationPublishEvent;

internal sealed record ProductsSynchronizationForcedData(ProductId Id, string Name, decimal Price, int Quantity);
