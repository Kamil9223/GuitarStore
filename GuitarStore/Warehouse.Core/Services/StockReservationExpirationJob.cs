using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warehouse.Core.Database;
using Warehouse.Core.Entities;

namespace Warehouse.Core.Services;

// TODO: Replace with distributed job mechanism (Hangfire / distributed lock)
// TODO: if application is scaled to multiple instances.
public class StockReservationExpirationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<StockReservationExpirationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireReservations(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while releasing stock reservation");
            }
        }
    }
    
    private async Task ExpireReservations(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var warehouseContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            
        var actualDateTime = DateTime.UtcNow;
            
        var reservations = await warehouseContext.ProductReservations
            .Where(r => r.Status == ReservationStatus.Active)
            .Where(r => r.ExpiresAtUtc < actualDateTime)
            .ToListAsync(stoppingToken);
            
        var productsOnStock = await warehouseContext.Stock
            .Where(x => reservations.Select(r => r.ProductId).Contains(x.ProductId))
            .ToListAsync(stoppingToken);
            
        foreach (var reservation in reservations)
        {
            var stockProduct = productsOnStock.Single(x => x.ProductId == reservation.ProductId);
            reservation.ExpireReservation(stockProduct);
        }
        
        await warehouseContext.SaveChangesAsync(stoppingToken);
    }
}