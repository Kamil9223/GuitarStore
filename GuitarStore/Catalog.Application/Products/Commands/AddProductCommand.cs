using Application.CQRS.Command;
using Catalog.Application.Abstractions;
using Catalog.Application.Products.Events.Outgoing;
using Catalog.Domain;
using Catalog.Domain.IRepositories;
using Common.EfCore.Transactions;
using Common.Errors.Exceptions;
using Common.RabbitMq.Abstractions.EventHandlers;
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

internal sealed class AddProductCommandHandler(
    IProductRepository productRepository,
    IVariationOptionRepository variationOptionRepository,
    IBrandRepository brandRepository,
    ICategoryRepository categoryRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    ITransactionExecutor<ICatalogUnitOfWork> transactionExecutor)
    : ICommandHandler<AddProductCommand>
{
    public async Task Handle(AddProductCommand command, CancellationToken ct)
    {
        await transactionExecutor.ExecuteAsync(async unitOfWork =>
        {
            var productAlreadyExists = await productRepository.Exists(x => x.Name == command.Name, ct);
            if (productAlreadyExists)
                throw new DomainException($"Product with Name: [{command.Name}] already exists.");

            var variationOptions = await variationOptionRepository.Get(command.VariationOptionIds, ct);

            if (variationOptions.Count != command.VariationOptionIds.Count)
                throw new DomainException("Not all of provided variation options exist.");

            var brand = await brandRepository.Get(command.BrandId, ct);
            if (brand is null)
                throw new DomainException($"Brand with Id = [{command.BrandId}] not exists.");

            var category = await categoryRepository.GetCategoryThatHasNotChildren(command.CategoryId, ct);
            if (category is null)
                throw new DomainException($"Category that has not children with Id = [{command.CategoryId}] not exists.");

            var product = new Product(command.Name, command.Description, command.Price, command.Quantity, brand, category, variationOptions);

            productRepository.Add(product);

            await unitOfWork.SaveChangesAsync(ct);

            await integrationEventPublisher.Publish(new ProductAddedEvent(product.Id, command.Name, command.Price, command.Quantity), ct);
        }, ct);
    }
}
