using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Orders.Application.Orders.Events.Outgoing;
internal sealed record CreatedOrderEvent(
    OrderId OrderId,
    decimal TotalAmount,
    Currency Currency)
    : IntegrationEvent, IIntegrationPublishEvent;
