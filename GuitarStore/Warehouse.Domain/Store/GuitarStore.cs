using Domain;
using Warehouse.Domain.AcousticGuitars;
using Warehouse.Domain.ElectricGuitars;

namespace Warehouse.Domain.Store;

public class GuitarStore : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public StoreLocation Location { get; }
    public ICollection<AcousticGuitar> AcousticGuitars { get; }
    public ICollection<ElectricGuitar> ElectricGuitars { get; }

    //For EF Core
    private GuitarStore() { }

    private GuitarStore(string name, StoreLocation location)
    {
        Name = name;
        Location = location;
        AcousticGuitars = new List<AcousticGuitar>();
        ElectricGuitars = new List<ElectricGuitar>();
    }

    public static GuitarStore Create(string name, StoreLocation storeLocation)
    {
        return new GuitarStore(name, storeLocation);
    }

    public void AddElectricGuitar(string companyName, string modelName, decimal price, ICollection<Pickup> pickups)
    {
        ElectricGuitars.Add(ElectricGuitar.Create(Id, companyName, modelName, price, pickups));
    }

    public void AddAcousticGuitar(string companyName, string modelName, decimal price)
    {
        AcousticGuitars.Add(AcousticGuitar.Create(Id, companyName, modelName, price));
    }


}
