using Domain;
using Warehouse.Domain.Products;

namespace Warehouse.Domain.Store;

public class GuitarStore : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; private set; }
    public string Street { get; private set; }
    public string PostalCode { get; private set; }
    public string City { get; private set; }
    public ICollection<Product> Products { get; }

    public GuitarStore() { }

    public GuitarStore(string name, string street, string postalCode, string city)
    {
        Name = name;
        Street = street;
        PostalCode = postalCode;
        City = city;
        Products = new List<Product>();
    }

    public void ChangeLocation(string street, string postalCode, string city)
    {
        Street = street;
        PostalCode = postalCode;
        City = city;
    }
}
