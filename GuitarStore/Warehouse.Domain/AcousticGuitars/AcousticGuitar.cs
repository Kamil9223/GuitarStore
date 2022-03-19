using Domain;
using Warehouse.Domain.Store;

namespace Warehouse.Domain.AcousticGuitars;

public class AcousticGuitar : GuitarBase, IIdentifiable
{
    public int Id { get; }
    public int GuitarStoreId { get; }
    public GuitarStore GuitarStore { get; }

    private AcousticGuitar(int guitarStoreId, string companyName, string modelName, decimal price)
        : base(companyName, modelName, price)
    {
        GuitarStoreId = guitarStoreId;
    }

    internal static AcousticGuitar Create(int guitarStoreId, string companyName, string modelName, decimal price)
    {
        return new AcousticGuitar(guitarStoreId, companyName, modelName, price);
    }
}
