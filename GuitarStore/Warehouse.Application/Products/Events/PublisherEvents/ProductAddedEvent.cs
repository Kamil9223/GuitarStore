using Infrastructure.RabbitMq.Abstractions.Events;

namespace Warehouse.Application.Products.Events.PublisherEvents;

internal class ProductAddedEvent : ApplicationEvent, IEventPublisher
{
}
