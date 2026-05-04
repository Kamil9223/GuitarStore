using Auth.Core.Data;
using Common.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Auth.Core.Outbox;

internal sealed class AuthOutboxReader(AuthDbContext dbContext) : IOutboxReader
{
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int take, CancellationToken ct)
        => await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < 5)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(take)
            .ToListAsync(ct);

    public Task SaveAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
