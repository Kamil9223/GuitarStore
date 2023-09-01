using Application.Exceptions;
using Application.RabbitMq.Abstractions;
using Customers.Application.Abstractions;
using Customers.Application.Customers.Events.Incoming;
using Customers.Domain.Customers;
using Domain.ValueObjects;

namespace Customers.Application.Customers.Handlers.EventHandlers;

internal class SignedUpEventHandler : IIntegrationEventHandler<SignedUpEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SignedUpEventHandler(ICustomerRepository productRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SignedUpEvent @event)
    {
        var customerExists = await _customerRepository.Exists(x => x.Email == @event.Email);
        if (customerExists)
            throw new GuitarStoreApplicationException($"Customer with Email: [{@event.Email}] already exists.");

        var validEmail = EmailAddress.Create(@event.Email);

        var customer = Customer.Create(@event.Name, @event.LastName, validEmail);

        _customerRepository.Add(customer);
        await _unitOfWork.SaveChanges();
    }
}
