using Application.CQRS.Command;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Domain.StronglyTypedIds;

namespace Customers.Application.Carts.Commands;
public sealed record RemoveCartItemCommand(CustomerId CustomerId, ProductId ProductId) : ICommand;

internal sealed class RemoveCartItemCommandHandler(
    ICartRepository cartRepository,
    ICustomersUnitOfWork unitOfWork) : ICommandHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand command, CancellationToken ct)
    {
        var cart = await cartRepository.GetCart(command.CustomerId, ct);
        cart.RemoveProduct(command.ProductId, quantity: 1);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
