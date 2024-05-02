using Application.RabbitMq.Abstractions.Events;

namespace Catalog.Application.Products.Events.Outgoing;

internal sealed record ProductAddedEvent(string Name, decimal Price, int Quantity) : IntegrationEvent, IIntegrationPublishEvent;
