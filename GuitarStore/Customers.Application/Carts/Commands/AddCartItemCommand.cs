using Application.CQRS.Command;
using Common.Errors.Exceptions;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Customers.Domain.Products;
using Domain.StronglyTypedIds;

namespace Customers.Application.Carts.Commands;
public sealed record AddCartItemCommand(ProductId ProductId, CustomerId CustomerId, string Name, decimal Price) : ICommand;

internal sealed class AddCartItemCommandHandler : ICommandHandler<AddCartItemCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public AddCartItemCommandHandler(ICartRepository cartRepository, IProductRepository productRepository, ICustomersUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddCartItemCommand command, CancellationToken ct)
    {
        var cart = await _cartRepository.GetCart(command.CustomerId, ct);
        var product = await _productRepository.Get(command.ProductId, ct);
        if (product is null)
            throw new NotFoundException(command.ProductId);

        cart.AddProduct(product, 1);

        await _cartRepository.Update(cart, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
