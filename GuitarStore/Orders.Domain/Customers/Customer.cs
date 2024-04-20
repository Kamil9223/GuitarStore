using Domain;
using Domain.ValueObjects;

namespace Orders.Domain.Customers;

public class Customer : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; } = null!;
    public string LastName { get; } = null!;
    public EmailAddress Email { get; } = null!;

    //For EF Core
    private Customer() { }

    private Customer(string name, string lastName, EmailAddress email)
    {
        Name = name;
        LastName = lastName;
        Email = email;
    }

    public static Customer Create(string name, string lastName, EmailAddress email)
    {
        return new Customer(name, lastName, email);
    }
}
