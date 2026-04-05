using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Abstractions.Events;
using Customers.Application.Abstractions;
using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Domain.ValueObjects;

namespace Customers.Application.Customers.Events.Incoming;

internal sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Name,
    string LastName,
    DateTimeOffset OccurredAtUtc)
    : IntegrationEvent, IIntegrationConsumeEvent;

internal sealed class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;
    private readonly ICartRepository _cartRepository;

    public UserRegisteredEventHandler(
        ICustomerRepository customerRepository,
        ICustomersUnitOfWork unitOfWork,
        ICartRepository cartRepository)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _cartRepository = cartRepository;
    }

    public async Task Handle(UserRegisteredEvent @event, CancellationToken ct)
    {
        var customerExists = await _customerRepository.Exists(x => x.AuthUserId == @event.UserId, ct);
        if (customerExists)
        {
            return;
        }

        var validEmail = EmailAddress.Create(@event.Email);
        var customer = Customer.Create(@event.UserId, @event.Name, @event.LastName, validEmail);

        _customerRepository.Add(customer);

        var cart = Cart.Create(customer.Id);
        await _cartRepository.Add(cart, ct);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
