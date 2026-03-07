using Common.RabbitMq.Abstractions.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Application.Abstractions;
using Orders.Application.Orders.Events.Outgoing;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.BackgroundJobs;

internal sealed class OrderExpirationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderExpirationJob> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public OrderExpirationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderExpirationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderExpirationJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireOrders(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while expiring orders.");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("OrderExpirationJob stopped.");
    }

    private async Task ExpireOrders(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IOrdersUnitOfWork>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        var expiredOrders = await orderRepository.GetExpiredPendingPaymentOrders(ct);

        if (expiredOrders.Count == 0)
            return;

        _logger.LogInformation("Found {Count} expired orders to process.", expiredOrders.Count);

        foreach (var order in expiredOrders)
        {
            try
            {
                order.Expire();
                await orderRepository.Update(order, ct);

                _logger.LogInformation(
                    "Order {OrderId} expired. Publishing OrderExpiredEvent.",
                    order.Id);

                await eventPublisher.Publish(
                    new OrderExpiredEvent(order.Id, "PaymentTimeout", DateTime.UtcNow),
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to expire Order {OrderId}.", order.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Successfully expired {Count} orders.", expiredOrders.Count);
    }
}
