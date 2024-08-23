using Domain.StronglyTypedIds;

namespace Warehouse.Core.Entities;
public class ProductReservation
{
    public OrderId OrderId { get; set; }

    public ProductId ProductId { get; set; }

    public int ReservedQuantity { get; set; }
}
