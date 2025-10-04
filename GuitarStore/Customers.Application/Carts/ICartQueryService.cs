using Customers.Application.Carts.Queries;
using Domain.StronglyTypedIds;

namespace Customers.Application.Carts;
public interface ICartQueryService
{
    Task<CartDetailsResponse> GetCartDetails(CustomerId customerId, CancellationToken ct);
}
