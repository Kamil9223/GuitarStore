namespace Orders.Infrastructure.Orders;
internal class OrderDbModel
{
    public int Id { get; }

    public int? CustomerId { get; set; }

    public string Object { get; set; } = null!;
}
