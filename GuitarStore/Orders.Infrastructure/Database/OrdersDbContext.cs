using Microsoft.EntityFrameworkCore;
using Orders.Domain.Customers;
using Orders.Domain.Products;
using Orders.Infrastructure.Instructions;
using Orders.Infrastructure.Orders;

namespace Orders.Infrastructure.Database;
internal class OrdersDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<OrderDbModel> Orders { get; set; } = null!;

    public DbSet<Instruction> Instructions { get; set; } = null!;

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }
}
