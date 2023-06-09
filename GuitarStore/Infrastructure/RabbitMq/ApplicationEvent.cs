using Domain.ValueObjects;

namespace Infrastructure.RabbitMq;

/// <summary>
/// Represents application event sending between modules in asynchronous way (e.g RabbitMq)
/// </summary>
public class ApplicationEvent : ValueObject
{
    /// <summary>
    /// Informs when event was created
    /// </summary>
    public DateTimeOffset EventCreationDate { get; init; }

    /// <summary>
    /// Used for tracking business transactions across the whole platform
    /// </summary>
    public Guid CorrelationId { get; init; }
}
