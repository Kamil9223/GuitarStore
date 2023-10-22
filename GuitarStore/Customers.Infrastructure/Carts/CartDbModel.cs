using Customers.Domain.Carts;

namespace Customers.Infrastructure.Carts;
internal class CartDbModel
{
    public int CustomerId { get; set; }

    public CartState CartState { get; set; }

    public string Object { get; set; }
}
