using Domain;

namespace Warehouse.Domain.Store;

public class StoreLocation : ValueObject
{
    public string Street { get; }
    public string PostalCode { get; }
    public string City { get; }

    //For EF Core
    private StoreLocation() { }

    private StoreLocation(string address, string postalCode, string city)
    {
        Street = address;
        PostalCode = postalCode;
        City = city;
    }

    public static StoreLocation Create(string address, string postalCode, string city)
    {
        //check rules

        return new StoreLocation(address, postalCode, city);
    }
}
