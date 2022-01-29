using Domain;

namespace Warehouse.Domain.ElectricGuitars;

public class ElectricGuitar : GuitarBase, IIdentifiable
{
    public int Id { get; }
    public int GuitarStoreId { get; }
    public ICollection<Pickup> Pickups { get; }

    private ElectricGuitar(int guitarStoreId, ICollection<Pickup> pickups)
    {
        GuitarStoreId = guitarStoreId;
        Pickups = pickups;
    }

    internal static ElectricGuitar Create(int guitarStoreId, ICollection<Pickup> pickups)
    {
        return new ElectricGuitar(guitarStoreId, pickups);
    }
}

