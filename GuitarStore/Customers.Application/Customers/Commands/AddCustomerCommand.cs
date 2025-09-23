using Application.CQRS.Command;
using Customers.Application.Abstractions;
using Customers.Domain.Customers;
using Domain.ValueObjects;

namespace Customers.Application.Customers.Commands;
public sealed record AddCustomerCommand(string Name, string LastName, string Email) : ICommand;

internal sealed class AddCustomerCommandHandler : ICommandHandler<AddCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomersUnitOfWork _unitOfWork;

    public AddCustomerCommandHandler(ICustomerRepository customerRepository, ICustomersUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddCustomerCommand command)
    {
        var customer = Customer.Create(command.Name, command.LastName, EmailAddress.Create(command.Email));

        _customerRepository.Add(customer);

        await _unitOfWork.SaveChangesAsync();
    }
}
