using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Carts;

internal class CartDbConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts", "Customers");

        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
        builder.Ignore(x => x.TotalPrice);

        builder.OwnsMany(x => x.CartItems, builder =>
        {
            builder.WithOwner();
            builder.ToTable("CartItems", "Customers");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(200);
            builder.Property(x => x.ProductId).HasColumnName("ProductId");
            builder.Property(x => x.Quantity).HasColumnName("Quantity");
            builder.OwnsOne(x => x.Price, builder =>
            {
                builder.Property(x => x.Value).HasColumnName("Price").HasColumnType("decimal(10,2)");
            });
        });

        builder.HasOne<Customer>()
            .WithOne();
    }
}
