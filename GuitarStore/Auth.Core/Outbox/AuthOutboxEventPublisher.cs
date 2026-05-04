using Auth.Core.Data;
using Common.Outbox;
using Common.RabbitMq.Abstractions.Events;
using Newtonsoft.Json;

namespace Auth.Core.Outbox;

internal sealed class AuthOutboxEventPublisher(AuthDbContext dbContext) : IAuthOutboxPublisher
{
    public async Task PublishToOutbox<TEvent>(TEvent @event, CancellationToken ct)
        where TEvent : IntegrationEvent, IIntegrationPublishEvent
    {
        var payload = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        var message = new OutboxMessage(@event.GetType().Name, payload, @event.CorrelationId.ToString());
        await dbContext.OutboxMessages.AddAsync(message, ct);
    }
}
