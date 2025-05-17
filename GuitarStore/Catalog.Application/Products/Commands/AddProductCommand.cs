using Application.CQRS;
using Application.RabbitMq.Abstractions;
using Catalog.Application.Abstractions;
using Catalog.Application.Products.Events.Outgoing;
using Catalog.Domain;
using Catalog.Domain.IRepositories;
using Domain.Exceptions;
using Domain.StronglyTypedIds;

namespace Catalog.Application.Products.Commands;

public sealed record AddProductCommand(
    string Name,
    string Description,
    CategoryId CategoryId,
    BrandId BrandId,
    ICollection<VariationOptionId> VariationOptionIds,
    decimal Price,
    int Quantity) : ICommand;

internal sealed class AddProductCommandHandler : ICommandHandler<AddProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IVariationOptionRepository _variationOptionRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public AddProductCommandHandler(
        IProductRepository productRepository,
        IVariationOptionRepository variationOptionRepository,
        IBrandRepository brandRepository,
        ICategoryRepository categoryRepository,
        ICatalogUnitOfWork unitOfWork,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _productRepository = productRepository;
        _variationOptionRepository = variationOptionRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(AddProductCommand command)
    {
        var productAlreadyExists = await _productRepository.Exists(x => x.Name == command.Name);
        if (productAlreadyExists)
            throw new DomainException($"Product with Name: [{command.Name}] already exists.");

        var variationOptions = await _variationOptionRepository.Get(command.VariationOptionIds);

        if (variationOptions.Count != command.VariationOptionIds.Count)
            throw new DomainException("Not all of provided variation options exist.");

        var brand = await _brandRepository.Get(command.BrandId);
        if (brand is null)
            throw new DomainException($"Brand with Id = [{command.BrandId}] not exists.");

        var category = await _categoryRepository.GetCategoryThatHasNotChildren(command.CategoryId);
        if (category is null)
            throw new DomainException($"Category that has not children with Id = [{command.CategoryId}] not exists.");

        var product = new Product(command.Name, command.Description, command.Price, command.Quantity, brand, category, variationOptions);

        _productRepository.Add(product);

        await _unitOfWork.SaveChangesAsync();

        await _integrationEventPublisher.Publish(new ProductAddedEvent(product.Id, command.Name, command.Price, command.Quantity));
    }
}
