using Customers.Domain.Carts;
using Domain.StronglyTypedIds;

namespace Customers.Infrastructure.Carts;
internal class CartDbModel
{
    public CustomerId CustomerId { get; set; }

    public CartState CartState { get; set; }

    public string Object { get; set; }
}
