using Orders.Domain.Orders;

namespace Orders.Infrastructure.Orders;
internal sealed class OrderReadModel
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required byte Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required decimal TotalPrice { get; init; }
    public required int ItemsCount { get; init; }
    public required DeliveryAddress DeliveryAddress { get; init; }
    public required string Deliverer { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

public sealed class OrderItemReadModel
{
    public required Guid Id { get; init; }
    public required Guid OrderId { get; init; }
    public required string ProductName { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required int Quantity { get; init; }
}
