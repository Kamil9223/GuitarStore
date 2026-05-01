using Microsoft.EntityFrameworkCore;

namespace Common.Outbox;

internal class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }
    
    public DbSet<OutboxMessage> OutboxMessages { get; init; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureOutbox();
    }
}