using Domain;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;

namespace Orders.Domain.Customers;

public class Customer : Entity
{
    public CustomerId Id { get; }
    public string Name { get; } = null!;
    public string LastName { get; } = null!;
    public EmailAddress Email { get; } = null!;

    //For EF Core
    private Customer() { }

    private Customer(CustomerId id, string name, string lastName, EmailAddress email)
    {
        Id = id;
        Name = name;
        LastName = lastName;
        Email = email;
    }

    public static Customer Create(CustomerId id, string name, string lastName, EmailAddress email)
    {
        return new Customer(id, name, lastName, email);
    }
}
