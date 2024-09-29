using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;

namespace Payments.Core.Events.Incoming;
internal sealed record CreatedOrderEvent() : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class CreatedOrderEventHandler : IIntegrationEventHandler<CreatedOrderEvent>
{
    public async Task Handle(CreatedOrderEvent @event)
    {
        
    }
}
