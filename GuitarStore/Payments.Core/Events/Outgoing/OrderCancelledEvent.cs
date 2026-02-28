using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Payments.Core.Events.Outgoing;

public sealed record OrderCancelledEvent(OrderId OrderId) : IntegrationEvent, IIntegrationPublishEvent;