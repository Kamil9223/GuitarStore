using Application.RabbitMq.Abstractions.Events;

namespace Catalog.Application.Products.Events.PublisherEvents;

internal class ProductAddedEvent : ApplicationEvent, IPublishEvent
{
}
