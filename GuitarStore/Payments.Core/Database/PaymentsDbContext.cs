using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Payments.Core.Entities;

namespace Payments.Core.Database;

internal class PaymentsDbContext : DbContext, IPaymentsDbContext
{
    public const string Schema = "Payments";

    public DbSet<ProcessedWebhookMessage> ProcessedWebhookMessages { get; set; }

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
    }
    
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) => Database.BeginTransactionAsync(ct);

    public override async Task<int> SaveChangesAsync(CancellationToken ct) => await base.SaveChangesAsync(ct);
}