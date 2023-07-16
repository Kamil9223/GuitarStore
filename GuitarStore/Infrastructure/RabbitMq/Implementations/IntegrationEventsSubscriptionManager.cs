using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;

namespace Infrastructure.RabbitMq.Implementations;

internal class IntegrationEventsSubscriptionManager
{
    private readonly Dictionary<Type, Type> _handlers;

    public IntegrationEventsSubscriptionManager()
    {
        _handlers = new Dictionary<Type, Type>();
    }

    public void AddSubscription<TEvent, TEventHandler>()
        where TEvent : IntegrationEvent, IIntegrationConsumeEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        if (_handlers.ContainsKey(typeof(TEvent)))
        {
            throw new ArgumentException($"Handler Type {typeof(TEventHandler).FullName} already registered for '{typeof(TEvent).FullName}'");
        }

        _handlers.Add(typeof(TEvent), typeof(TEventHandler));
    }

    public Type GetHandlerTypeForEvent(Type eventType) => _handlers[eventType];
}
