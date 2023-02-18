﻿using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Categories;
using Warehouse.Domain.Products;
using Warehouse.Domain.Store;

namespace Warehouse.Infrastructure.Database;

internal class WarehouseDbContext : DbContext
{
    public DbSet<GuitarStore> GuitarStores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public WarehouseDbContext() { }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}
