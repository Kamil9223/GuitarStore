using Domain.StronglyTypedIds;

namespace Warehouse.Core.Entities;
public class ProductReservation
{
    public OrderId OrderId { get; set; } = null!;

    public ProductId ProductId { get; set; } = null!;//TODO: dedicated ValueObjectId

    public int ReservedQuantity { get; set; }
}
