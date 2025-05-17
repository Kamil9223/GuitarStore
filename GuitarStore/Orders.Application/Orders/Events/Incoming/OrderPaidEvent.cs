using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Common.EfCore.Transactions;
using Domain.StronglyTypedIds;
using Orders.Application.Abstractions;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.Events.Incoming;
internal sealed record OrderPaidEvent(OrderId OrderId) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class OrderPaidEventHandler : IIntegrationEventHandler<OrderPaidEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;

    public OrderPaidEventHandler(IOrderRepository orderRepository, IOrdersUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderPaidEvent @event)
    {
        var order = await _orderRepository.Get(@event.OrderId);
        order.PayOrder();
        await _unitOfWork.SaveChangesAsync();
    }
}
