using Application.Exceptions;
using Domain;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Categories;
using Warehouse.Application.Products.Commands;
using Warehouse.Domain.Categories;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

internal class ProductCommandHandler :
    ICommandHandler<AddProductCommand>,
    ICommandHandler<DeleteProductCommand>,
    ICommandHandler<UpdateProductCommand>
{
    private readonly IRepository<Product> _productsRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductCommandHandler(IRepository<Product> productsRepository, IRepository<Category> categoryRepository)
    {
        _productsRepository = productsRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task Handle(AddProductCommand command)
    {
        var category = await _categoryRepository.SingleOrDefault(CategorySpecification.IsTheMostNestedCategory(command.CategoryId));
        if (category is null)
        {
            throw new NotFoundException($"Cannot Add product with category: cateogryId: [{command.CategoryId}] bacause the category is not the most nested category");
        }

        var product = new Product
        {
            Brand = command.Brand,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            Category = category
        };

        await _productsRepository.Add(product);
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
        var existingProduct = await _productsRepository.Get(command.Id);
        if (existingProduct is null)
        {
            throw new NotFoundException($"Cannot Modyfie product with Id: [{command.Id}] because the product not exists.");
        }

        existingProduct.UpdateProduct(command.Description, command.Price);
    }
}
