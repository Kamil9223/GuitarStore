using Customers.Domain.Carts;
using Domain;
using Domain.ValueObjects;

namespace Customers.Domain.Customers;

public class Customer : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public string LastName { get; }
    public EmailAddress Email { get; }
    public CustomerAddress Address { get; }
    public Cart Cart { get; }

    //For EF Core
    private Customer() { }

    private Customer(string name, string lastName, EmailAddress email, CustomerAddress address, Cart cart)
    {
        Name = name;
        LastName = lastName;
        Email = email;
        Address = address;
        Cart = cart;
    }

    public static Customer Create(string name, string lastName, EmailAddress email, CustomerAddress address)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Provided property [Name]: [{name}] is invalid.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException($"Provided property [LastName]: [{lastName}] is invalid.");
        }

        return new Customer(name, lastName, email, address, Cart.Create());
    }
}
