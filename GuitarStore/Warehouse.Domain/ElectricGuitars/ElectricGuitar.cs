using Domain;
using Warehouse.Domain.Store;

namespace Warehouse.Domain.ElectricGuitars;

public class ElectricGuitar : GuitarBase, IIdentifiable
{
    public int Id { get; }
    public int GuitarStoreId { get; }
    public GuitarStore GuitarStore { get; }
    public ICollection<Pickup> Pickups { get; }

    //For EF Core
    private ElectricGuitar() { }

    private ElectricGuitar(int guitarStoreId, string companyName, string modelName, decimal price, ICollection<Pickup> pickups)
        : base(companyName, modelName, price)
    {
        GuitarStoreId = guitarStoreId;
        Pickups = pickups;
    }

    internal static ElectricGuitar Create(int guitarStoreId, string companyName, string modelName, decimal price, ICollection<Pickup> pickups)
    {
        return new ElectricGuitar(guitarStoreId, companyName, modelName, price, pickups);
    }
}

