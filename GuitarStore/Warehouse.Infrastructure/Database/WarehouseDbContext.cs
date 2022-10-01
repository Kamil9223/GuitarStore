using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Product;
using Warehouse.Domain.Store;

namespace Warehouse.Infrastructure.Database;

internal class WarehouseDbContext : DbContext
{
    public DbSet<GuitarStore> GuitarStores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public const string DbSchema = "Warehouse";

    public WarehouseDbContext() { }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}
