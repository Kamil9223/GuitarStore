using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Outbox;
public sealed class OutboxMessage
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Payload { get; init; }
    public required DateTime OccurredOnUtc { get; init; }
    public required string? CorrelationId { get; init; }
    public required DateTime? ProcessedOnUtc { get; set; }
    public required int RetryCount { get; set; }
}
