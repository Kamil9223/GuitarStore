using Catalog.Application.Abstractions;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Catalog.Infrastructure.Database;

internal class CatalogDbContext : DbContext, ICatalogDbContext
{
    public const string Schema = "Catalog";

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Variation> Variations { get; set; }
    public DbSet<VariationOption> VariationOptions { get; set; }

    public CatalogDbContext() { }

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync() => Database.BeginTransactionAsync();

    public async Task SaveChangesAsync() => await base.SaveChangesAsync();
}
