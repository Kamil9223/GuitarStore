using Common.RabbitMq.Abstractions.Events;

namespace Auth.Core.Events.Outgoing;

internal sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Name,
    string LastName,
    DateTimeOffset OccurredAtUtc)
    : IntegrationEvent, IIntegrationPublishEvent;
