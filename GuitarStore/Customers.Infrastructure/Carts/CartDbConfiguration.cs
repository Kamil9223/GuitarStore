using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Carts;

internal class CartDbConfiguration : IEntityTypeConfiguration<CartDbModel>
{
    public void Configure(EntityTypeBuilder<CartDbModel> builder)
    {
        builder.ToTable("Carts", "Customers");

        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.Object).HasColumnType("jsonb").IsRequired();

        builder.Property(x => x.CartState).HasConversion(
                x => x.ToString(),
                x => (CartState)Enum.Parse(typeof(CartState), x));

        builder.HasOne<Customer>()
            .WithOne();
    }
}
