using Customers.Domain.Customers;
using Customers.Domain.Products;
using Customers.Infrastructure.Carts;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests.EndToEnd")]
namespace Customers.Infrastructure.Database;

internal class CustomersDbContext : DbContext
{
    public const string Schema = "Customers";

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CartDbModel> Carts { get; set; }

    public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);
    }
}
