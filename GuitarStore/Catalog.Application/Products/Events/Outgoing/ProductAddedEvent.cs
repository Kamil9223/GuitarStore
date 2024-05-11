using Application.RabbitMq.Abstractions.Events;

namespace Catalog.Application.Products.Events.Outgoing;

internal sealed record ProductAddedEvent(int Id, string Name, decimal Price, int Quantity) : IntegrationEvent, IIntegrationPublishEvent;
