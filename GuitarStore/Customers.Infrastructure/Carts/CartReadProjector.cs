using Common.Errors.Exceptions;
using Customers.Application;
using Customers.Domain.Carts;
using Customers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Carts;
internal sealed class CartReadProjector
    (CustomersDbContext customersDbContext)
    : ICartReadProjector
{
    public async Task Upsert(Cart cart, CancellationToken ct)
    {
        var id = cart.CustomerId.Value;
        var itemsCount = cart.CartItems.Count;
        var total = cart.TotalPrice;
        var state = cart.CartItems.Count != 0 ? CartState.ContainingProducts : CartState.Empty;

        var exists = await customersDbContext.CartReadModels
            .AnyAsync(x => x.CustomerId == id, ct);

        if (!exists)
        {
            await customersDbContext.CartReadModels.AddAsync(new CartReadModel
            {
                CustomerId = id,
                CartState = state,
                TotalPrice = total,
                ItemsCount = itemsCount,
                Deliverer = null
            }, ct);
        }
        else
        {
            await customersDbContext.CartReadModels
                .Where(x => x.CustomerId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.CartState, _ => state)
                    .SetProperty(x => x.TotalPrice, _ => total)
                    .SetProperty(x => x.ItemsCount, _ => itemsCount)
                    .SetProperty(x => x.Deliverer, _ => null),
                    ct);
        }

        await customersDbContext.CartItemReadModels
            .Where(x => x.CustomerId == id)
            .ExecuteDeleteAsync(ct);

        if (cart.CartItems.Count > 0)
        {
            var newItems = cart.CartItems.Select(i => new CartItemReadModel
            {
                CustomerId = id,
                ProductId = i.ProductId.Value
            });

            await customersDbContext.CartItemReadModels.AddRangeAsync(newItems, ct);
        }
    }

    public async Task Upsert(CheckoutCart cart, CancellationToken ct)
    {
        var id = cart.CustomerId.Value;
        var itemsCount = cart.CartItems.Count;
        var total = cart.CartItems.Sum(x => x.Price);

        var exists = await customersDbContext.CartReadModels
            .AnyAsync(x => x.CustomerId == id, ct);

        if (!exists)
            throw new NotFoundException(cart.CustomerId);

        await customersDbContext.CartReadModels
            .Where(x => x.CustomerId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.CartState, _ => CartState.Checkouted)
                .SetProperty(x => x.TotalPrice, _ => total)
                .SetProperty(x => x.ItemsCount, _ => itemsCount)
                .SetProperty(x => x.Deliverer, _ => cart.Delivery.Deliverer),
                ct);
    }
}
