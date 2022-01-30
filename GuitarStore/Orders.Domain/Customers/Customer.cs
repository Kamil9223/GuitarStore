using Domain;

namespace Orders.Domain.Customers;

public class Customer : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public string LastName { get; }
    public string Email { get; }

    private Customer(string name, string lastName, string email)
    {
        Name = name;
        LastName = lastName;
        Email = email;
    }

    public static Customer Create(string name, string lastName, string email)
    {
        //check rules?

        return new Customer(name, lastName, email);
    }

}
