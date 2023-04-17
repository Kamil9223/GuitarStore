using Application.Exceptions;
using Domain;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Products.Commands;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

internal class ProductCommandHandler :
    ICommandHandler<AddProductCommand>,
    ICommandHandler<DeleteProductCommand>,
    ICommandHandler<UpdateProductCommand>
{
    private readonly IRepository<Product> _productsRepository;

    public ProductCommandHandler(IRepository<Product> productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task Handle(AddProductCommand command)
    {
        //sprawdź czy istnieje taka kategoria i czy jest ona najbaedziej zagnieżdżoną
        var product = new Product
        {

        };
    }

    public async Task Handle(DeleteProductCommand command)
    {
        var existingProduct = await _productsRepository.Get(command.Id);
        if (existingProduct is null)
        {
            throw new NotFoundException($"Cannot Delete product with Id: [{command.Id}] because the product not exists.");
        }

        await _productsRepository.Remove(existingProduct);
    }

    public async Task Handle(UpdateProductCommand command)
    {
        
    }
}
