using Common.Outbox;
using Common.RabbitMq.Abstractions.Events;
using Newtonsoft.Json;
using Payments.Core.Database;

namespace Payments.Core.Outbox;

internal sealed class PaymentsOutboxEventPublisher(PaymentsDbContext dbContext) : IPaymentsOutboxPublisher
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
