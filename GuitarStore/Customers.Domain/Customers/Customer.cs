using Customers.Domain.Carts;
using Domain;

namespace Customers.Domain.Customers;

public class Customer : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public string LastName { get; }
    public string Email { get; }
    public CustomerAddress Address { get; }
    public Cart Cart { get; }

    private Customer(string name, string lastName, string email, CustomerAddress address)
    {
        Name = name;
        LastName = lastName;
        Email = email;
        Address = address;
        Cart = Cart.Create();
    }

    public static Customer Create(string name, string lastName, string email, CustomerAddress address)
    {
        //check rules?

        return new Customer(name, lastName, email, address);
    }
}
