using Application.RabbitMq.Abstractions.Events;

namespace Customers.Application.Customers.Events.Incoming;

internal class SignedUpEvent : IntegrationEvent, IIntegrationConsumeEvent
{
    public string Name { get; }
    public string LastName { get; }
    public string Email { get; }
}
