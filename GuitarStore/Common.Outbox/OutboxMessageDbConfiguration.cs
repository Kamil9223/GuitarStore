using Microsoft.EntityFrameworkCore;

namespace Common.Outbox;

public static class OutboxMessageDbConfiguration
{
    public static void ConfigureOutbox(this ModelBuilder modelBuilder, string schema)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages", schema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.OccurredOnUtc).IsRequired();
            builder.Property(x => x.CorrelationId).HasMaxLength(100);
            builder.Property(x => x.ProcessedOnUtc);
            builder.Property(x => x.RetryCount).IsRequired();
            builder.Property(x => x.LastError).HasMaxLength(2000);
            builder.HasIndex(x => x.ProcessedOnUtc);
        });
    }
}