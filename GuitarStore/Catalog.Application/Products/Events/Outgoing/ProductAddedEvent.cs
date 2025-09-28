using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Catalog.Application.Products.Events.Outgoing;

internal sealed record ProductAddedEvent(ProductId Id, string Name, decimal Price, int Quantity) : IntegrationEvent, IIntegrationPublishEvent;
