using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Delivery.Core.Events.Incoming;
internal sealed record OrderPaidEvent(OrderId OrderId) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class OrderPaidEventHandler : IIntegrationEventHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent @event)
    {
        throw new NotImplementedException();//TODO
    }
}
