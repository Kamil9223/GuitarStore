using Common.Errors.Exceptions;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Warehouse.Core.Database;
using Warehouse.Core.Entities;
using Warehouse.Shared;

namespace Warehouse.Core.InternalModuleApi;
internal class ProductReservationService : IProductReservationService
{
    private readonly WarehouseDbContext _dbContext;

    public ProductReservationService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ReserveProducts(ReserveProductsDto dto, CancellationToken ct)//TODO: Transaction Wrapper !!!
    {
        var productIds = dto.Products.Select(x => x.ProductId).ToList();
        var productsOnStock = await _dbContext.Stock.Where(x => productIds.Contains(x.ProductId)).ToListAsync(ct);

        var missingProducts = productIds
            .Except(productsOnStock.Select(x => x.ProductId))
            .ToList();
        
        if (missingProducts.Count != 0)
            throw new DomainException($"Critical! Products with ids: [{string.Join(", ", missingProducts)}] does not exists on Stock!");

        var existingSet = (await _dbContext.ProductReservations
                .Where(r => r.OrderId == dto.OrderId)
                .Where(r => r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Confirmed)
                .Select(r => r.ProductId)
                .ToListAsync(ct))
            .ToHashSet();

        foreach (var product in dto.Products)
        {
            if (existingSet.Contains(product.ProductId))
                continue;
            
            var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE [Warehouse].[Stock]
                SET Quantity = Quantity - {product.Quantity}
                WHERE ProductId = {product.ProductId.Value}
                  AND Quantity >= {product.Quantity};
            ", ct);

            if (affected == 0)
                throw new DomainException("The requested quantity exceeds the available stock.");
            
            _dbContext.ProductReservations.Add(new ProductReservation(
                dto.OrderId,
                product.ProductId,
                product.Quantity,
                dto.TimeToLive));
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ConfirmReservations(OrderId orderId, CancellationToken ct)
    {
        var reservations = await _dbContext.ProductReservations
            .Where(x => x.OrderId == orderId)
            .Where(x => x.Status == ReservationStatus.Active)
            .ToListAsync(ct);

        if (reservations.Count == 0)
            return;

        foreach (var reservation in reservations)
            reservation.ConfirmReservation();
        
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ReleaseReservations(OrderId orderId, CancellationToken ct)
    {
        var reservations = await _dbContext.ProductReservations
            .Where(x => x.OrderId == orderId)
            .Where(x => x.Status == ReservationStatus.Active)
            .ToListAsync(ct);

        if (reservations.Count == 0)
            return;
        
        var productsOnStock = await _dbContext.Stock
            .Where(x => reservations.Select(r => r.ProductId).Contains(x.ProductId))
            .ToListAsync(ct);

        foreach (var reservation in reservations)
        {
            var stockProduct = productsOnStock.Single(x => x.ProductId == reservation.ProductId);
            reservation.ReleaseReservation(stockProduct);
        }
        
        await _dbContext.SaveChangesAsync(ct);
    }
}