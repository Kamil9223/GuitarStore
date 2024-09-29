using Application.RabbitMq.Abstractions.Events;

namespace Orders.Application.Orders.Events.Outgoing;
internal sealed record CompleteOrderEvent()
    : IntegrationEvent, IIntegrationPublishEvent;
