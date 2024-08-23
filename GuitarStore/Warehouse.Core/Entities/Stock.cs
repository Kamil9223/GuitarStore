using Domain;
using Domain.StronglyTypedIds;

namespace Warehouse.Core.Entities;
public class Stock : Entity
{
    public ProductId ProductId { get; set; }

    public int Quantity { get; set; }
}
