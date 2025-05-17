using Application.CQRS;
using Common.EfCore.Transactions;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using static Customers.Application.Carts.Commands.CheckoutCartCommand;

namespace Customers.Application.Carts.Commands;
public sealed record CheckoutCartCommand(CustomerId CustomerId, DeliveryCommandPart Delivery) : ICommand
{
    public sealed record DeliveryCommandPart(DelivererId DelivererId, string Deliverer);
}

internal sealed class CheckoutCartCommandHandler : ICommandHandler<CheckoutCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public CheckoutCartCommandHandler(ICartRepository cartRepository, ICustomersUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CheckoutCartCommand command)
    {
        var cart = await _cartRepository.GetCart(command.CustomerId);

        var checkout = cart.Checkout();

        checkout.SetModelOfDelivery(new Delivery(
            delivererId: command.Delivery.DelivererId,
            deliverer: command.Delivery.Deliverer));

        await _cartRepository.Update(checkout);
        await _unitOfWork.SaveChangesAsync();
    }
}
