using Common.RabbitMq.Abstractions.Events;
using Domain.StronglyTypedIds;

namespace Payments.Core.Events.Outgoing;

public sealed record OrderPaidEvent(OrderId OrderId) : IntegrationEvent, IIntegrationPublishEvent;
