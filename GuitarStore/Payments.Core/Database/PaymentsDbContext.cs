using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Payments.Core.Entities;

namespace Payments.Core.Database;

internal class PaymentsDbContext : DbContext, IPaymentsDbContext
{
    public const string Schema = "Payments";

    public DbSet<ProcessedWebhookMessage> ProcessedWebhookMessages { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedWebhookMessage>(b =>
        {
            b.ToTable("ProcessedWebhookMessages", Schema);

            b.HasKey(x => x.Id);

            b.Property(x => x.EventId).HasMaxLength(500).IsRequired();
            b.HasIndex(x => x.EventId).IsUnique();

            b.Property(x => x.EventType).HasMaxLength(300).IsRequired();

            b.Property(x => x.Status).IsRequired();

            b.Property(x => x.ReceivedAtUtc).IsRequired();

            b.Property(x => x.Error).HasMaxLength(2000);
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages", Schema);

            b.HasKey(x => x.Id);

            b.Property(x => x.Type).HasMaxLength(255).IsRequired();

            b.Property(x => x.Payload).IsRequired();

            b.Property(x => x.OccurredOnUtc).IsRequired();

            b.Property(x => x.CorrelationId).HasMaxLength(100);

            b.Property(x => x.ProcessedOnUtc);

            b.Property(x => x.RetryCount).IsRequired();

            b.Property(x => x.LastError).HasMaxLength(2000);

            b.HasIndex(x => x.ProcessedOnUtc);
        });
    }
    
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) => Database.BeginTransactionAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct) => await base.SaveChangesAsync(ct);
}