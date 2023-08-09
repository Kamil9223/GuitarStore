using Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Application.Products.Commands;
using Catalog.Domain;
using Catalog.Domain.IRepositories;

namespace Catalog.Application.Products.CommandHandlers;

internal class AddProductCommandHandler : ICommandHandler<AddProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IVariationOptionRepository _variationOptionRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductCommandHandler(
        IProductRepository productRepository,
        IVariationOptionRepository variationOptionRepository,
        IBrandRepository brandRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _variationOptionRepository = variationOptionRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddProductCommand command)
    {
        var productAlreadyExists = await _productRepository.Exists(x => x.Name == command.Name);
        if (productAlreadyExists)
            throw new GuitarStoreApplicationException($"Product with Name: [{command.Name}] already exists.");

        var variationOptions = await _variationOptionRepository.Get(command.VariationOptionIds);

        if (variationOptions.Count != command.VariationOptionIds.Count)
            throw new GuitarStoreApplicationException("Not all of provided variation options exist.");

        var brand = await _brandRepository.Get(command.BrandId);
        if (brand is null)
            throw new GuitarStoreApplicationException($"Brand with Id = [{command.BrandId}] not exists.");

        var category = await _categoryRepository.GetCategoryThatHasNotChildren(command.CategoryId);
        if (category is null)
            throw new GuitarStoreApplicationException($"Category that has not children with Id = [{command.CategoryId}] not exists.");

        var product = new Product(command.Name, command.Description, brand, category, variationOptions);

        _productRepository.Add(product);

        await _unitOfWork.SaveChanges();

        //publish event
    }
}
