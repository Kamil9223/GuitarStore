using Domain.StronglyTypedIds;
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

            builder.Property(e => e.OrderId)
                .HasConversion(
                    id => id!.Value,
                    value => new OrderId(value));

            builder.Property(e => e.ProductId)
                .HasConversion(
                    id => id!.Value,
                    value => new ProductId(value));
        });

        modelBuilder.Entity<Stock>(builder =>
        {
            builder.ToTable("Stock", "Warehouse");
            builder.HasKey(e => e.ProductId);

            builder.Property(e => e.ProductId)
               .HasConversion(
                   id => id!.Value,
                   value => new ProductId(value));
        });
    }
}
