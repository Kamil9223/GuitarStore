using Application.RabbitMq.Abstractions.Events;

namespace Application.RabbitMq.Abstractions;

public interface IIntegrationEventHandler<TEvent>
    where TEvent : IntegrationEvent, IIntegrationConsumeEvent
{
    Task Handle(TEvent @event);
}
