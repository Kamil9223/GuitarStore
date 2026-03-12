using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Warehouse;
using Tests.EndToEnd.Setup.TestsHelpers;
using Warehouse.Core.Entities;
using Warehouse.Core.Events.Incoming;
using Xunit;

namespace Tests.EndToEnd.E2E_Warehouse.EventHandlers;

public sealed class WarehouseEventHandlersTests(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task OrderPaidEvent_ShouldConfirmReservation()
    {
        // Arrange
        var orderId = OrderId.New();
        var productId = ProductId.New();
        var stock = Databases.WarehouseDbContext.SeedStock(productId, 10);
        var reservation = Databases.WarehouseDbContext.SeedReservation(orderId, productId, 2);
        await Databases.WarehouseDbContext.SaveChangesAsync();

        var orderPaidEvent = new OrderPaidEvent(orderId);

        // Act
        RabbitMqChannel.PublishTestEvent(orderPaidEvent);

        // Assert
        await Waiter.WaitForCondition(async () =>
        {
            Databases.WarehouseDbContext.ChangeTracker.Clear();
            var updatedReservation = await Databases.WarehouseDbContext.ProductReservations
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == productId);

            return updatedReservation?.Status == ReservationStatus.Confirmed;
        }, TimeSpan.FromSeconds(5));

        var finalReservation = await Databases.WarehouseDbContext.ProductReservations
            .SingleAsync(r => r.OrderId == orderId && r.ProductId == productId);
        finalReservation.Status.ShouldBe(ReservationStatus.Confirmed);
    }

    [Fact]
    public async Task OrderCancelledEvent_ShouldReleaseReservationAndRestoreStock()
    {
        // Arrange
        var orderId = OrderId.New();
        var productId = ProductId.New();
        var initialStockQuantity = 10;
        var reservedQuantity = 2;
        
        var stock = Databases.WarehouseDbContext.SeedStock(productId, initialStockQuantity);
        var reservation = Databases.WarehouseDbContext.SeedReservation(orderId, productId, reservedQuantity);
        await Databases.WarehouseDbContext.SaveChangesAsync();

        var orderCancelledEvent = new OrderCancelledEvent(
            OrderId: orderId,
            Reason: "User cancelled",
            CancelledBy: "User",
            OccurredAtUtc: DateTime.UtcNow);

        // Act
        RabbitMqChannel.PublishTestEvent(orderCancelledEvent);

        // Assert
        await Waiter.WaitForCondition(async () =>
        {
            Databases.WarehouseDbContext.ChangeTracker.Clear();
            var updatedReservation = await Databases.WarehouseDbContext.ProductReservations
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == productId);

            var updatedStock = await Databases.WarehouseDbContext.Stock
                .FirstOrDefaultAsync(s => s.ProductId == productId);

            return updatedReservation?.Status == ReservationStatus.Released && 
                   updatedStock?.Quantity == initialStockQuantity + reservedQuantity;
        }, TimeSpan.FromSeconds(5));

        var finalReservation = await Databases.WarehouseDbContext.ProductReservations
            .SingleAsync(r => r.OrderId == orderId && r.ProductId == productId);
        finalReservation.Status.ShouldBe(ReservationStatus.Released);

        var finalStock = await Databases.WarehouseDbContext.Stock.SingleAsync(s => s.ProductId == productId);
        finalStock.Quantity.ShouldBe(initialStockQuantity + reservedQuantity);
    }

    [Fact]
    public async Task OrderExpiredEvent_ShouldReleaseReservationAndRestoreStock()
    {
        // Arrange
        var orderId = OrderId.New();
        var productId = ProductId.New();
        var initialStockQuantity = 10;
        var reservedQuantity = 2;

        var stock = Databases.WarehouseDbContext.SeedStock(productId, initialStockQuantity);
        var reservation = Databases.WarehouseDbContext.SeedReservation(orderId, productId, reservedQuantity);
        await Databases.WarehouseDbContext.SaveChangesAsync();

        var orderExpiredEvent = new OrderExpiredEvent(
            OrderId: orderId,
            Reason: "TTL expired",
            OccurredAtUtc: DateTime.UtcNow);

        // Act
        RabbitMqChannel.PublishTestEvent(orderExpiredEvent);

        // Assert
        await Waiter.WaitForCondition(async () =>
        {
            Databases.WarehouseDbContext.ChangeTracker.Clear();
            var updatedReservation = await Databases.WarehouseDbContext.ProductReservations
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == productId);

            var updatedStock = await Databases.WarehouseDbContext.Stock
                .FirstOrDefaultAsync(s => s.ProductId == productId);

            return updatedReservation?.Status == ReservationStatus.Released &&
                   updatedStock?.Quantity == initialStockQuantity + reservedQuantity;
        }, TimeSpan.FromSeconds(5));

        var finalReservation = await Databases.WarehouseDbContext.ProductReservations
            .SingleAsync(r => r.OrderId == orderId && r.ProductId == productId);
        finalReservation.Status.ShouldBe(ReservationStatus.Released);

        var finalStock = await Databases.WarehouseDbContext.Stock.SingleAsync(s => s.ProductId == productId);
        finalStock.Quantity.ShouldBe(initialStockQuantity + reservedQuantity);
    }
}
