using Common.RabbitMq.Abstractions.Events;

namespace Common.Outbox;

public interface IOutboxEventPublisher
{
    Task PublishToOutbox<TEvent>(TEvent @event, CancellationToken ct)
        where TEvent : IntegrationEvent, IIntegrationPublishEvent;
}
