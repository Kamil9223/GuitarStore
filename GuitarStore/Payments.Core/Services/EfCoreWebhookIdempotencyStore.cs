using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payments.Core.Database;
using Payments.Core.Entities;

namespace Payments.Core.Services;

public interface IWebhookIdempotencyStore
{
    Task<IdempotencyConsumeResult> TryConsumeAsync(
        string eventId,
        DateTimeOffset createdUtc,
        CancellationToken ct);

    Task MarkCompletedAsync(string eventId, CancellationToken ct);

    Task MarkFailedAsync(string eventId, string error, CancellationToken ct);
}

public enum IdempotencyConsumeResult
{
    Consumed,
    Duplicate,
    ExpiredNoOp
}

internal sealed class EfCoreWebhookIdempotencyStore(
    PaymentsDbContext dbContext,
    IOptions<WebhookTimeToLiveConfiguration> ttlOption,
    ILogger<EfCoreWebhookIdempotencyStore> logger) : IWebhookIdempotencyStore
{
    public async Task<IdempotencyConsumeResult> TryConsumeAsync(
        string eventId,
        DateTimeOffset createdUtc,
        CancellationToken ct)
    {
        if (ttlOption.Value.IsExpired(createdUtc))
        {
            logger.LogWarning(
                "Webhook event {EventId} is expired (created {CreatedUtc}). TTL exceeded.",
                eventId, createdUtc);

            return IdempotencyConsumeResult.ExpiredNoOp;
        }
        
        var entity = new ProcessedWebhookMessage(
            eventId: eventId,
            eventType: "unknown",
            orderId: null);

        dbContext.ProcessedWebhookMessages.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(ct);
            return IdempotencyConsumeResult.Consumed;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogInformation(
                "Duplicate webhook event detected: {EventId}",
                eventId);

            return IdempotencyConsumeResult.Duplicate;
        }
    }

    public async Task MarkCompletedAsync(string eventId, CancellationToken ct)
    {
        var entity = await dbContext.ProcessedWebhookMessages
            .SingleAsync(x => x.EventId == eventId, ct);

        entity.Status = WebhookProcessingStatus.Completed;
        entity.ProcessedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(string eventId, string error, CancellationToken ct)
    {
        var entity = await dbContext.ProcessedWebhookMessages
            .SingleAsync(x => x.EventId == eventId, ct);

        entity.Status = WebhookProcessingStatus.Failed;
        entity.Error = error;
        entity.ProcessedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
            return sqlEx.Number is 2601 or 2627;

        return false;
    }
}