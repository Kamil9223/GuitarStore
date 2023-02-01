using Domain;
using Domain.ValueObjects;

namespace Warehouse.Domain.Store;

public class StoreLocation : ValueObject
{
    public string Street { get; }
    public string PostalCode { get; }
    public string City { get; }

    //For EF Core
    private StoreLocation() { }

    private StoreLocation(string street, string postalCode, string city)
    {
        Street = street;
        PostalCode = postalCode;
        City = city;
    }

    public static StoreLocation Create(string street, string postalCode, string city)
    {
        if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(postalCode) || string.IsNullOrWhiteSpace(city))
        {
            throw new DomainException($"At least one of [{nameof(StoreLocation)}] property is empty.");
        }

        return new StoreLocation(street, postalCode, city);
    }
}
