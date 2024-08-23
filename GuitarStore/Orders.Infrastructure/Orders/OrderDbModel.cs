using Domain.StronglyTypedIds;

namespace Orders.Infrastructure.Orders;
internal class OrderDbModel
{
    public OrderId Id { get; }

    public CustomerId? CustomerId { get; set; }

    public string Object { get; set; } = null!;
}
