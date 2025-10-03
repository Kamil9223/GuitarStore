using Common.RabbitMq.Abstractions.Events;

namespace Common.RabbitMq.Abstractions.EventHandlers;

public interface IIntegrationEventHandler<TEvent>
    where TEvent : IntegrationEvent, IIntegrationConsumeEvent
{
    Task Handle(TEvent @event, CancellationToken ct);
}
