using Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Application.Categories;
using Catalog.Application.Products.Commands;
using Catalog.Domain;
using Domain;

namespace Catalog.Application.Products;

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
        var isProductWithBrandAndNameExists = await _productsRepository.Any(ProductSpecification.WithBrandAndName(command.Brand, command.Name));
        if (isProductWithBrandAndNameExists)
        {
            throw new GuitarStoreApplicationException($"Product with Brand: [{command.Brand}] and Name: [{command.Name}] already exists.");
        }

        var category = await _categoryRepository.SingleOrDefault(CategorySpecification.IsTheMostNestedCategory(command.CategoryId));
        if (category is null)
        {
            throw new GuitarStoreApplicationException($"Cannot Add product with category: cateogryId: [{command.CategoryId}] bacause the category is not the most nested category.");
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
