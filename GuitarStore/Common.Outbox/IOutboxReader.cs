namespace Common.Outbox;

public interface IOutboxReader
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int take, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
