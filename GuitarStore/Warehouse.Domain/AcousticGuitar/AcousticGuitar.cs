using Domain;

namespace Warehouse.Domain.AcousticGuitar;

public class AcousticGuitar : GuitarBase, IIdentifiable
{
    public int Id { get; }
}
