using Application.CQRS.Command;
using Common.Errors.Exceptions;
using Common.RabbitMq.Abstractions.EventHandlers;
using Domain.StronglyTypedIds;
using Orders.Application.Abstractions;
using Orders.Application.Orders.Events.Outgoing;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.Commands;

public sealed record CancelOrderCommand(
    OrderId OrderId,
    CustomerId CustomerId,
    string? Reason
) : ICommand;

internal sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IOrdersUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await orderRepository.Get(command.OrderId, ct);
        
        if (order.CustomerId != command.CustomerId)
            throw new DomainException("You can only cancel your own orders.");
        
        order.Cancel();

        await orderRepository.Update(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Publish business event
        await eventPublisher.Publish(
            new OrderCancelledEvent(
                command.OrderId,
                command.Reason ?? "User cancellation",
                "User",
                DateTime.UtcNow
            ),
            ct
        );
    }
}
