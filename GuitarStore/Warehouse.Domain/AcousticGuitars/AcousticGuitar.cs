using Domain;

namespace Warehouse.Domain.AcousticGuitars;

public class AcousticGuitar : GuitarBase, IIdentifiable
{
    public int Id { get; }
    public int GuitarStoreId { get; }

    private AcousticGuitar(int guitarStoreId)
    {
        GuitarStoreId = guitarStoreId;
    }

    internal static AcousticGuitar Create(int guitarStoreId)
    {
        return new AcousticGuitar(guitarStoreId);
    }
}
