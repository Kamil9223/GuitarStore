using Domain;
using Warehouse.Domain.Product;

namespace Warehouse.Domain.Store;

public class GuitarStore : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; private set; }
    public string Street { get; }
    public string PostalCode { get; }
    public string City { get; }
    public ICollection<Product.Product> Products { get; }
}
