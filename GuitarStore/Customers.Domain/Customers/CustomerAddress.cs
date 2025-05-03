using Domain;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Customers.Domain.Customers;

public class CustomerAddress : ValueObject
{
    public string Country { get; }
    public Locality? Locality { get; }
    public string LocalityName { get; }
    public string PostalCode { get; }
    public string HouseNumber { get; }
    public string Street { get; }
    public string LocalNumber { get; }

    private CustomerAddress(string country, Locality? locality, string localityName, string postalCode, string houseNumber, string street, string localNumber)
    {
        Country = country;
        Locality = locality;
        LocalityName = localityName;
        PostalCode = postalCode;
        HouseNumber = houseNumber;
        Street = street;
        LocalNumber = localNumber;
    }

    public static CustomerAddress Create(string country, Locality locality, string localityName, string postalCode, string houseNumber, string street, string localNumber)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            throw new DomainException($"Provided country: [{country}] is invalid.");
        }

        if (string.IsNullOrWhiteSpace(localityName))
        {
            throw new DomainException($"Provided localityName: [{localityName}] is invalid.");
        }

        if (string.IsNullOrWhiteSpace(houseNumber))
        {
            throw new DomainException($"Provided houseNumber: [{houseNumber}] is invalid.");
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            throw new DomainException($"Provided postalCode: [{postalCode}] is invalid.");
        }

        return new CustomerAddress(country, locality, localityName, postalCode, houseNumber, street, localNumber);
    }
}
