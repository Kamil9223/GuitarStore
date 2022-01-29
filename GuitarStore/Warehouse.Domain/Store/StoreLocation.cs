using Domain;

namespace Warehouse.Domain.Store;

public class StoreLocation : ValueObject
{
    public string Address { get; }
    public string PostalCode { get; }
    public string City { get; }

    private StoreLocation(string address, string postalCode, string city)
    {
        Address = address;
        PostalCode = postalCode;
        City = city;
    }

    public static StoreLocation Create(string address, string postalCode, string city)
    {
        //check rules

        return new StoreLocation(address, postalCode, city);
    }
}
