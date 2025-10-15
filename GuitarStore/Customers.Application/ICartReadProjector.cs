using Customers.Domain.Carts;

namespace Customers.Application;
public interface ICartReadProjector
{
    Task Upsert(Cart cart, CancellationToken ct);

    Task Upsert(CheckoutCart cart, CancellationToken ct);
}
