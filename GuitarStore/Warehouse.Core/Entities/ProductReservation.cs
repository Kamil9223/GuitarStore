namespace Warehouse.Core.Entities;
public class ProductReservation
{
    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }//TODO: dedicated ValueObjectId

    public int ReservedQuantity { get; private set; }


}
