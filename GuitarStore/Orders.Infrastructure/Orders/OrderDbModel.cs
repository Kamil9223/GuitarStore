namespace Orders.Infrastructure.Orders;
internal class OrderDbModel
{
    public int Id { get; set; }

    public int? CustomerId { get; }

    public string Object { get; set; } = null!;
}
