using Domain;

namespace Warehouse.Domain.ElectricGuitar;

public class ElectricGuitar : GuitarBase, IIdentifiable
{
    public int Id { get; }
    public ICollection<Pickup> Pickups { get; }
}

