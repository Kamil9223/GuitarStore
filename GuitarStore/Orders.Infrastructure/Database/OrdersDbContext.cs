using Microsoft.EntityFrameworkCore;
using Orders.Domain.Customers;
using Orders.Domain.Products;
using Orders.Infrastructure.Orders;

namespace Orders.Infrastructure.Database;
internal class OrdersDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<OrderDbModel> Orders { get; set; }

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }
}
