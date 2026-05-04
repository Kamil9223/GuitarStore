using Common.RabbitMq.Abstractions.EventHandlers;
using Common.RabbitMq.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Common.Outbox;

internal sealed class OutboxMessageDispatcherJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxMessageDispatcherJob> _logger;
    private static readonly TimeSpan DispatchInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetryCount = 5;

    public OutboxMessageDispatcherJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxMessageDispatcherJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Message Dispatcher Job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages.");
            }

            try
            {
                await Task.Delay(DispatchInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown requested
                break;
            }
        }

        _logger.LogInformation("Outbox Message Dispatcher Job stopped.");
    }

    private async Task ProcessOutboxMessages(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var readers = scope.ServiceProvider.GetRequiredService<IEnumerable<IOutboxReader>>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        foreach (var reader in readers)
        {
            await ProcessReader(reader, eventPublisher, ct);
        }
    }

    private async Task ProcessReader(IOutboxReader reader, IIntegrationEventPublisher eventPublisher, CancellationToken ct)
    {
        var messages = await reader.GetPendingAsync(50, ct);

        if (messages.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} outbox messages from {Reader}.", messages.Count, reader.GetType().Name);

        foreach (var message in messages)
        {
            try
            {
                var @event = DeserializeEvent(message.Payload);

                if (@event is null)
                {
                    _logger.LogWarning("Failed to deserialize outbox message {MessageId} of type {Type}. Marking as processed.",
                        message.Id, message.Type);
                    message.MarkAsProcessed();
                    continue;
                }

                if (@event is not IIntegrationPublishEvent publishEvent)
                {
                    _logger.LogWarning("Outbox message {MessageId} does not implement IIntegrationPublishEvent. Marking as processed.",
                        message.Id);
                    message.MarkAsProcessed();
                    continue;
                }

                await eventPublisher.Publish((dynamic)publishEvent, ct);
                message.MarkAsProcessed();

                _logger.LogDebug("Outbox message {MessageId} published successfully.", message.Id);
            }
            catch (Exception ex)
            {
                message.IncrementRetryCount(ex.Message);
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}. Retry count: {RetryCount}",
                    message.Id, message.RetryCount);

                if (message.RetryCount >= MaxRetryCount)
                {
                    _logger.LogError("Outbox message {MessageId} exceeded max retry count. Moving to dead letter.",
                        message.Id);
                }
            }
        }

        await reader.SaveAsync(ct);
    }

    private IntegrationEvent? DeserializeEvent(string payload)
    {
        try
        {
            return JsonConvert.DeserializeObject<IntegrationEvent>(payload, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize event from payload.");
            return null;
        }
    }
}
