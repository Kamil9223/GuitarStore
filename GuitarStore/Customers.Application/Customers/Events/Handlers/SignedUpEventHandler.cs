using Application.Exceptions;
using Application.RabbitMq.Abstractions;
using Customers.Application.Abstractions;
using Customers.Application.Customers.Events.Incoming;
using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Domain.ValueObjects;

namespace Customers.Application.Customers.Events.Handlers;

internal class SignedUpEventHandler : IIntegrationEventHandler<SignedUpEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartRepository _cartRepository;

    public SignedUpEventHandler(ICustomerRepository productRepository, IUnitOfWork unitOfWork, ICartRepository cartRepository)
    {
        _customerRepository = productRepository;
        _unitOfWork = unitOfWork;
        _cartRepository = cartRepository;
    }

    public async Task Handle(SignedUpEvent @event)
    {
        var customerExists = await _customerRepository.Exists(x => x.Email == @event.Email);
        if (customerExists)
            throw new GuitarStoreApplicationException($"Customer with Email: [{@event.Email}] already exists.");

        var validEmail = EmailAddress.Create(@event.Email);

        using var dbTransaction = await _unitOfWork.BeginTransaction();

        var customer = Customer.Create(@event.Name, @event.LastName, validEmail);
        _customerRepository.Add(customer);
        await _unitOfWork.SaveChanges();
        var cart = Cart.Create(customer.Id);
        await _cartRepository.Add(cart);

        dbTransaction.Commit();
    }
}
