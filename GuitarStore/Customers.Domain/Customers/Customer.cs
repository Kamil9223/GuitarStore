using Domain;

namespace Customers.Domain.Customers;

public class Customer : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public string LastName { get; }
    public string Email { get; }
    public CustomerAddress Address { get; }
}
