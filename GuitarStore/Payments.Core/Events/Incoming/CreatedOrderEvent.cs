using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Payments.Core.Events.Incoming;
internal sealed record CreatedOrderEvent(
    OrderId OrderId,
    decimal TotalAmount,
    Currency Currency) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class CreatedOrderEventHandler : IIntegrationEventHandler<CreatedOrderEvent>
{

    public async Task Handle(CreatedOrderEvent @event)
    {

    }
}
