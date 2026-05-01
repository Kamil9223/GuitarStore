using Common.RabbitMq.Abstractions.Events;

namespace Auth.Core.Events.Outgoing;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Name,
    string LastName,
    DateTimeOffset OccurredAtUtc)
    : IntegrationEvent, IIntegrationPublishEvent;
