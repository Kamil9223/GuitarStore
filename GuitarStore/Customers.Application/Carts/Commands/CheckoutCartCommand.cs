using Application.CQRS.Command;
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
    private readonly ICartReadProjector _cartReadProjector;

    public CheckoutCartCommandHandler(
        ICartRepository cartRepository,
        ICustomersUnitOfWork unitOfWork,
        ICartReadProjector cartReadProjector)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _cartReadProjector = cartReadProjector;
    }

    public async Task Handle(CheckoutCartCommand command, CancellationToken ct)
    {
        var cart = await _cartRepository.GetCart(command.CustomerId, ct);

        var checkout = cart.Checkout();

        checkout.SetModelOfDelivery(new Delivery(
            delivererId: command.Delivery.DelivererId,
            deliverer: command.Delivery.Deliverer));

        await _cartRepository.Update(checkout, ct);
        await _cartReadProjector.Upsert(checkout, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
