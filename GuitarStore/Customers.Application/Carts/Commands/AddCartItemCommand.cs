using Application.CQRS;
using Application.Exceptions;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Customers.Domain.Products;

namespace Customers.Application.Carts.Commands;
public sealed record AddCartItemCommand(int ProductId, int CustomerId, string Name, decimal Price) : ICommand;

internal sealed class AddCartItemCommandHandler : ICommandHandler<AddCartItemCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCartItemCommandHandler(ICartRepository cartRepository, IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddCartItemCommand command)
    {
        var cart = await _cartRepository.GetCart(command.CustomerId);
        var product = await _productRepository.Get(command.ProductId);
        if (product is null)
            throw new NotFoundException($"Product with Id: {command.ProductId} not exists.");

        cart.AddProduct(product, 1);

        await _cartRepository.Update(cart);
        await _unitOfWork.SaveChanges();
    }
}
