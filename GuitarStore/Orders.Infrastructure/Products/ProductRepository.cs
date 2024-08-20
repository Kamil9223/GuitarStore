using Application.Exceptions;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Products;
using Orders.Infrastructure.Database;

namespace Orders.Infrastructure.Products;

internal class ProductRepository : IProductRepository
{
    private readonly OrdersDbContext _context;

    public ProductRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Product> Get(ProductId id)
        => await _context.Products.SingleOrDefaultAsync(x => x.Id == id)
        ?? throw new NotFoundException($"Product with Id: {id} not exists.");
}
