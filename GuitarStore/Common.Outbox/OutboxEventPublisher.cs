using Common.RabbitMq.Abstractions.Events;
using Newtonsoft.Json;

namespace Common.Outbox;

public interface IOutboxEventPublisher
{
    Task PublishToOutbox<TEvent>(TEvent @event, CancellationToken ct)
        where TEvent : IntegrationEvent, IIntegrationPublishEvent;
}

internal sealed class OutboxEventPublisher(OutboxDbContext dbContext) : IOutboxEventPublisher
{
    public async Task PublishToOutbox<TEvent>(TEvent @event, CancellationToken ct)
        where TEvent : IntegrationEvent, IIntegrationPublishEvent
    {
        var eventType = @event.GetType().Name;
        var payload = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        var outboxMessage = new OutboxMessage(eventType, payload, @event.CorrelationId.ToString());

        await dbContext.AddAsync(outboxMessage, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}