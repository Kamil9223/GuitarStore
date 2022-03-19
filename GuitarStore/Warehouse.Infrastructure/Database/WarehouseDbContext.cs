using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.AcousticGuitars;
using Warehouse.Domain.ElectricGuitars;
using Warehouse.Domain.Store;

namespace Warehouse.Infrastructure.Database;

internal class WarehouseDbContext : DbContext
{
    public DbSet<GuitarStore> GuitarStores { get; set; }
    public DbSet<ElectricGuitar> ElectricGuitars { get; set; }
    public DbSet<AcousticGuitar> AcousticGuitars { get; set; }

    public const string DbSchema = "Warehouse";

    public WarehouseDbContext() { }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}
