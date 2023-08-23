using Customers.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Customers;

internal class CustomerDbConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "Customers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);

        builder.OwnsOne(x => x.Address, builder =>
        {
            builder.Property(x => x.Country).HasColumnName("Country").HasMaxLength(300);
            builder.Property(x => x.Locality).HasConversion(
                x => x.ToString(),
                x => (Locality)Enum.Parse(typeof(Locality), x))
                .HasColumnName("Locality").HasMaxLength(15);
            builder.Property(x => x.LocalityName).HasColumnName("LocalityName").HasMaxLength(500);
            builder.Property(x => x.PostalCode).HasColumnName("PostalCode").HasMaxLength(10);
            builder.Property(x => x.HouseNumber).HasColumnName("HouseNumber").HasMaxLength(20);
            builder.Property(x => x.Street).HasColumnName("Street").HasMaxLength(500);
            builder.Property(x => x.LocalNumber).HasColumnName("LocalNumber").HasMaxLength(20);
        });

        builder.OwnsOne(x => x.Email, builder =>
        {
            builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100);
        });

        builder.OwnsOne(x => x.Cart, builder =>
        {
            builder.WithOwner();
            builder.ToTable("Carts", "Customers");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

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
        });
    }
}
