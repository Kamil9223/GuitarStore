using Common.Errors.Exceptions;
using Customers.Application.Carts;
using Customers.Application.Carts.Queries;
using Customers.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Carts;
internal sealed class CartQueryService : ICartQueryService
{
    private readonly CustomersDbContext _context;

    public CartQueryService(CustomersDbContext context)
    {
        _context = context;
    }

    public async Task<CartDetailsResponse> GetCartDetails(CustomerId customerId, CancellationToken ct)
    {
        return await _context.CartReadModels
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId.Value)
            .Select(c => new CartDetailsResponse
            {
                CustomerId = c.CustomerId,
                TotalPrice = c.TotalPrice,
                Deliverer = c.Deliverer,
                CartState = c.CartState,
                ItemsCount = c.ItemsCount,
                Items = _context.CartItemReadModels
                    .AsNoTracking()
                    .Where(ci => ci.CustomerId == c.CustomerId)
                    .Join(_context.Products.AsNoTracking(),
                          ci => ci.ProductId,
                          p => (Guid)p.Id,
                          (ci, p) => new CartDetailsResponse.CartItem
                          {
                              ProductId = ci.ProductId,
                              ProductName = p.Name,
                              Price = p.Price,
                              Quantity = p.Quantity
                          })
                    .ToList()
            }).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(customerId);

    }
}
