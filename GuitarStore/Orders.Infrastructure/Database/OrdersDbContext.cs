﻿using Microsoft.EntityFrameworkCore;
using Orders.Domain.Customers;
using Orders.Domain.Products;
using Orders.Infrastructure.Orders;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests.EndToEnd")]
namespace Orders.Infrastructure.Database;
internal class OrdersDbContext : DbContext
{
    public const string Schema = "Orders";

    public DbSet<Customer> Customers { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<OrderDbModel> Orders { get; set; } = null!;

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }
}
