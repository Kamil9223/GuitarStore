using Common.Errors.Exceptions;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Orders;
using Orders.Application.Orders.Queries;
using Orders.Infrastructure.Database;
using static Orders.Application.Orders.Queries.OrdersHistoryResponse;

namespace Orders.Infrastructure.Orders;
internal sealed class OrderQueryService : IOrderQueryService
{
    private readonly OrdersDbContext _context;

    public OrderQueryService(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<OrderStatusResponse> GetOrderStatus(OrderId orderId, CancellationToken ct)
    {
        return await _context.OrderReadModels
            .AsNoTracking()
            .Where(o => o.Id == orderId.Value)
            .Select(o => new OrderStatusResponse
            {
                Status = o.Status
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(orderId);
    }

    public async Task<OrderDetailsResponse> GetOrderDetails(OrderId orderId, CancellationToken ct)
    {
        return await _context.OrderReadModels
            .AsNoTracking()
            .Where(o => o.Id == orderId.Value)
            .GroupJoin(
                _context.OrderItemReadModels.AsNoTracking(),
                o => o.Id,
                i => i.OrderId,
                (order, items) => new { order, items }
            )
            .Select(x => new OrderDetailsResponse
            {
                Id = x.order.Id,
                CustomerId = x.order.CustomerId,
                Deliverer = x.order.Deliverer,
                CreatedAt = x.order.CreatedAt,
                Status = x.order.Status,
                UpdatedAt = x.order.UpdatedAt,
                ItemsCount = x.order.ItemsCount,
                TotalPrice = x.order.TotalPrice,
                DeliveryAddress = new OrderDetailsResponse.Address
                {
                    Country = x.order.DeliveryAddress.Country,
                    HouseNumber = x.order.DeliveryAddress.HouseNumber,
                    LocalityName = x.order.DeliveryAddress.LocalityName,
                    LocalNumber = x.order.DeliveryAddress.LocalNumber,
                    PostalCode = x.order.DeliveryAddress.PostalCode,
                    Street = x.order.DeliveryAddress.Street,
                },
                Items = x.items
                    .OrderBy(i => i.Name)
                    .Select(i => new OrderDetailsResponse.OrderItem
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Price = i.Price,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(orderId);
    }

    public async Task<OrdersHistoryResponse> GetOrdersHistory(CustomerId customerId, CancellationToken ct)
    {
        var items = await _context.OrderReadModels
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId.Value)
            .Select(o => new OrderHistoryItem
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                UpdatedAt = o.UpdatedAt,
                Deliverer = o.Deliverer,
                ItemsCount = o.ItemsCount,
                TotalPrice = o.TotalPrice,
            })
            .ToListAsync(ct);

        return new OrdersHistoryResponse { Items = items };
    }
}
