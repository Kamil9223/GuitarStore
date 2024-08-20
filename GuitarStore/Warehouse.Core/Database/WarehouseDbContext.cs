using Microsoft.EntityFrameworkCore;
using Warehouse.Core.Entities;

namespace Warehouse.Core.Database;
internal class WarehouseDbContext : DbContext
{
    public DbSet<ProductReservation> ProductReservations { get; set; } = null!;
    public DbSet<Stock> Stock {  get; set; } = null!;

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductReservation>(builder =>
        {
            builder.ToTable("ProductReservations", "Warehouse");
            builder.HasKey(e => new { e.OrderId, e.ProductId });
        });

        modelBuilder.Entity<Stock>(builder =>
        {
            builder.ToTable("Stock", "Warehouse");
            builder.HasKey(e => e.ProductId);
        });
    }
}
