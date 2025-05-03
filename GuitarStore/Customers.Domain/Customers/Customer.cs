using Domain;
using Domain.Exceptions;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Customers.Domain.Customers;

public class Customer : Entity
{
    public CustomerId Id { get; }
    public string Name { get; }
    public string LastName { get; }
    public EmailAddress Email { get; }
    public CustomerAddress Address { get; }

    //For EF Core
    private Customer() { }

    private Customer(string name, string lastName, EmailAddress email, CustomerAddress address)
    {
        Id = CustomerId.New();
        Name = name;
        LastName = lastName;
        Email = email;
        Address = address;
    }

    public static Customer Create(string name, string lastName, EmailAddress email, CustomerAddress address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Provided property [Name]: [{name}] is invalid.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException($"Provided property [LastName]: [{lastName}] is invalid.");
        }

        return new Customer(name, lastName, email, address);
    }
}
