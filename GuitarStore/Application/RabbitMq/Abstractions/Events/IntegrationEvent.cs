using Domain.ValueObjects;

namespace Application.RabbitMq.Abstractions.Events;

/// <summary>
/// Represents application event sending between modules in asynchronous way (e.g RabbitMq)
/// </summary>
public class IntegrationEvent : ValueObject
{
    /// <summary>
    /// Informs when event was created
    /// </summary>
    public DateTimeOffset EventCreationDate { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Used for tracking business transactions across the whole platform
    /// </summary>
    public Guid CorrelationId { get; } = Guid.NewGuid();
}
