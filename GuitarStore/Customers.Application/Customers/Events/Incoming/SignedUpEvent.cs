using Application.RabbitMq.Abstractions;
using Application.RabbitMq.Abstractions.Events;
using Common.Errors.Exceptions;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Domain.ValueObjects;

namespace Customers.Application.Customers.Events.Incoming;

internal sealed record SignedUpEvent(string Name, string LastName, string Email) : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class SignedUpEventHandler : IIntegrationEventHandler<SignedUpEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;
    private readonly ICartRepository _cartRepository;

    public SignedUpEventHandler(ICustomerRepository productRepository, ICustomersUnitOfWork unitOfWork, ICartRepository cartRepository)
    {
        _customerRepository = productRepository;
        _unitOfWork = unitOfWork;
        _cartRepository = cartRepository;
    }

    public async Task Handle(SignedUpEvent @event)
    {
        var customerExists = await _customerRepository.Exists(x => x.Email == @event.Email);
        if (customerExists)
            throw new DomainException($"Customer with Email: [{@event.Email}] already exists.");

        var validEmail = EmailAddress.Create(@event.Email);

        var customer = Customer.Create(@event.Name, @event.LastName, validEmail);
        _customerRepository.Add(customer);
        var cart = Cart.Create(customer.Id);
        await _cartRepository.Add(cart);

        await _unitOfWork.SaveChangesAsync();
    }
}
