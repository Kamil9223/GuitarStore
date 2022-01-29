using Domain;

namespace Customers.Domain.Customers;

public class CustomerAddress : ValueObject
{
    public string Country { get; }
    public Locality Locality { get; }
    public string HouseNumber { get; }
    public string? Street { get; }
    public string? LocalNumber { get; }

    private CustomerAddress(string country, Locality locality, string houseNumber, string? street, string? localNumber)
    {
        Country = country;
        Locality = locality;
        HouseNumber = houseNumber;
        Street = street;
        LocalNumber = localNumber;
    }

    public static CustomerAddress Create(string country, Locality locality, string houseNumber, string? street, string? localNumber)
    {
        //Check rules

        return new CustomerAddress(country, locality, houseNumber, street, localNumber);
    }
}
