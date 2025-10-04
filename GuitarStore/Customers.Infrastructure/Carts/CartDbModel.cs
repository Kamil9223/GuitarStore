using Customers.Domain.Carts;
using Domain.StronglyTypedIds;

namespace Customers.Infrastructure.Carts;
internal class CartDbModel
{
    public required CustomerId CustomerId { get; set; }

    public required CartState CartState { get; set; }

    public required string Object { get; set; }
}
