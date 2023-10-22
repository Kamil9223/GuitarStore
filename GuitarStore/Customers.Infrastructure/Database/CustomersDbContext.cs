using Customers.Domain.Customers;
using Customers.Domain.Products;
using Customers.Infrastructure.Carts;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Database;

internal class CustomersDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CartDbModel> Carts { get; set; }

    public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);
    }
}
