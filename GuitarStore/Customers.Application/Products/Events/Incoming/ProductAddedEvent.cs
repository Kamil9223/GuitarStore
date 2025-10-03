using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Abstractions.Events;
using Customers.Application.Abstractions;
using Customers.Domain.Products;
using Domain.StronglyTypedIds;

namespace Customers.Application.Products.Events.Incoming;

internal sealed record ProductAddedEvent(ProductId Id, string Name, decimal Price, int Quantity) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class ProductAddedEventHandler : IIntegrationEventHandler<ProductAddedEvent>
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public ProductAddedEventHandler(IProductRepository productRepository, ICustomersUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProductAddedEvent @event, CancellationToken ct)//TODO: do podnoszenia quantity raczej osobny event. Tutaj zastanowić się nad upsertem
    {
        var existedProduct = await _productRepository.Get(@event.Id, ct);
        if (existedProduct is not null)
        {
            existedProduct.IncreaseQuantity(@event.Quantity);
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        var product = Product.Create(@event.Id, @event.Name, @event.Price, @event.Quantity);
        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
