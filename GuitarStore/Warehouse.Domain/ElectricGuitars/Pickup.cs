using Domain;

namespace Warehouse.Domain.ElectricGuitars;

public class Pickup : Entity
{
    public int Id { get; }
    public string Name { get; }
    public PickupType PickupType { get; }
    public ICollection<ElectricGuitar> ElectricGuitars { get; }

    private Pickup(string name, PickupType pickupType)
    {
        Name = name;
        PickupType = pickupType;
    }

    public static Pickup Create(string name, PickupType pickupType)
    {
        //Check rules?

        return new Pickup(name, pickupType);
    }
}
