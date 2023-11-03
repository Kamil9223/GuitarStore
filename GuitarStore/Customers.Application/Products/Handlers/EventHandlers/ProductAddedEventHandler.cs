using Application.Exceptions;
using Application.RabbitMq.Abstractions;
using Customers.Application.Abstractions;
using Customers.Application.Products.Events.Incoming;
using Customers.Domain.Products;

namespace Customers.Application.Products.Handlers.EventHandlers;

internal class ProductAddedEventHandler : IIntegrationEventHandler<ProductAddedEvent>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductAddedEventHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProductAddedEvent @event)
    {
        var productExists = await _productRepository.Exists(p => p.Name == @event.Name);
        if (productExists)
            throw new GuitarStoreApplicationException($"Product with Name: [{@event.Name}] already exists.");

        var product = Product.Create(default, @event.Name, @event.Price, @event.Quantity);

        _productRepository.Add(product);

        await _unitOfWork.SaveChanges();
    }
}
