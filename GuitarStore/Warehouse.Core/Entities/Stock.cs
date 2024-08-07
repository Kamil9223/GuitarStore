using Domain;

namespace Warehouse.Core.Entities;
public class Stock : Entity
{
    public Guid ProductId { get; private set; }//TODO: dedicated ValueObjectId

    public int Quantity { get; private set; }
}
