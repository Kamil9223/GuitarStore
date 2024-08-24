using Customers.Domain.Carts;
using Customers.Domain.Customers;
using Customers.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Carts;

internal class CartDbConfiguration : IEntityTypeConfiguration<CartDbModel>
{
    public void Configure(EntityTypeBuilder<CartDbModel> builder)
    {
        builder.ToTable("Carts", CustomersDbContext.Schema);

        builder.HasKey(x => x.CustomerId);

        builder.Property(e => e.CustomerId)
               .HasConversion(
                   id => id!.Value,
                   value => new CustomerId(value));

        builder.Property(x => x.Object).IsRequired();

        builder.Property(x => x.CartState).HasConversion(
                x => x.ToString(),
                x => (CartState)Enum.Parse(typeof(CartState), x));

        builder.HasOne<Customer>()
            .WithOne();
    }
}
