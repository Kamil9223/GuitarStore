using Application.CQRS;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Domain.StronglyTypedIds;
using static Customers.Application.Carts.Commands.CheckoutCartCommand;

namespace Customers.Application.Carts.Commands;
public sealed record CheckoutCartCommand(CustomerId CustomerId, PaymentCommandPart Payment, DeliveryCommandPart Delivery) : ICommand
{
    public sealed record PaymentCommandPart(PaymentId PaymentId, string PaymentType);
    public sealed record DeliveryCommandPart(DelivererId DelivererId, string Deliverer);
}

internal sealed class CheckoutCartCommandHandler : ICommandHandler<CheckoutCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutCartCommandHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CheckoutCartCommand command)
    {
        var cart = await _cartRepository.GetCart(command.CustomerId);

        var checkout = cart.Checkout();

        checkout.SetMethodOfPayment(new Payment(
            paymentId: command.Payment.PaymentId,
            paymentType: command.Payment.PaymentType));

        checkout.SetModelOfDelivery(new Delivery(
            delivererId: command.Delivery.DelivererId,
            deliverer: command.Delivery.Deliverer));

        await _cartRepository.Update(checkout);
        await _unitOfWork.SaveChanges();
    }
}
