﻿using Application.CQRS;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using static Customers.Application.Carts.Commands.CheckoutCartCommand;

namespace Customers.Application.Carts.Commands;
public sealed record CheckoutCartCommand(CustomerId CustomerId, PaymentMethod Payment, DeliveryCommandPart Delivery) : ICommand
{
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

        checkout.Payment = command.Payment;

        checkout.SetModelOfDelivery(new Delivery(
            delivererId: command.Delivery.DelivererId,
            deliverer: command.Delivery.Deliverer));

        await _cartRepository.Update(checkout);
        await _unitOfWork.SaveChanges();
    }
}
