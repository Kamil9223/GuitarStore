using Domain.ValueObjects;

namespace Orders.Domain.Orders;
public class DeliveryAddress : ValueObject
{
    public string Country { get; }
    public string LocalityName { get; }
    public string PostalCode { get; }
    public string HouseNumber { get; }
    public string Street { get; }
    public string? LocalNumber { get; }

    private DeliveryAddress() { }

    public DeliveryAddress(string country, string localityName, string postalCode, string houseNumber, string street, string? localNumber)
    {
        Country = country;
        LocalityName = localityName;
        PostalCode = postalCode;
        HouseNumber = houseNumber;
        Street = street;
        LocalNumber = localNumber;
    }
}
