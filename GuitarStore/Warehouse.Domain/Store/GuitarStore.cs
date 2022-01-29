using Domain;

namespace Warehouse.Domain.Store;

public class GuitarStore : Entity, IIdentifiable
{
    public int Id { get; }
    public string Name { get; }
    public StoreLocation Location { get; }
    public ICollection<AcousticGuitar.AcousticGuitar> AcousticGuitars { get; }
    public ICollection<ElectricGuitar.ElectricGuitar> ElectricGuitars { get; }

    private GuitarStore(string name, StoreLocation location)
    {
        Name = name;
        Location = location;
        AcousticGuitars = new List<AcousticGuitar.AcousticGuitar>();
        ElectricGuitars = new List<ElectricGuitar.ElectricGuitar>();
    }

    public static GuitarStore Create(string name, StoreLocation storeLocation)
    {
        return new GuitarStore(name, storeLocation);
    }


}
