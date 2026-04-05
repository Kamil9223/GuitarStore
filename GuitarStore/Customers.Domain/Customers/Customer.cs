using Common.Errors.Exceptions;
using Domain;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Customers.Domain.Customers;

public class Customer : Entity
{
    public CustomerId Id { get; }
    public Guid AuthUserId { get; }
    public string Name { get; }
    public string LastName { get; }
    public EmailAddress Email { get; }
    public CustomerAddress Address { get; }

    //For EF Core
    private Customer() { }

    private Customer(Guid authUserId, string name, string lastName, EmailAddress email, CustomerAddress address)
    {
        Id = CustomerId.New();
        AuthUserId = authUserId;
        Name = name;
        LastName = lastName;
        Email = email;
        Address = address;
    }

    public static Customer Create(Guid authUserId, string name, string lastName, EmailAddress email, CustomerAddress address = null)
    {
        if (authUserId == Guid.Empty)
        {
            throw new DomainException($"Provided property [AuthUserId]: [{authUserId}] is invalid.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Provided property [Name]: [{name}] is invalid.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException($"Provided property [LastName]: [{lastName}] is invalid.");
        }

        return new Customer(authUserId, name, lastName, email, address);
    }
}
