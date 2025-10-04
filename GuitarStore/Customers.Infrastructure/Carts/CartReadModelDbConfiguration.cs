using Customers.Domain.Carts;
using Customers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Carts;
internal class CartReadModelDbConfiguration : IEntityTypeConfiguration<CartReadModel>
{
    public void Configure(EntityTypeBuilder<CartReadModel> builder)
    {
        builder.ToTable("Carts_Read", CustomersDbContext.Schema);
        builder.HasKey(x => x.CustomerId);
        builder.Property(x => x.CartState).HasConversion(
            x => x.ToString(),
            x => (CartState)Enum.Parse(typeof(CartState), x));
    }
}

internal class CartItemReadModelDbConfiguration : IEntityTypeConfiguration<CartItemReadModel>
{
    public void Configure(EntityTypeBuilder<CartItemReadModel> builder)
    {
        builder.ToTable("CartItems_Read", CustomersDbContext.Schema);
        builder.HasKey(x => x.CustomerId);
        builder.HasIndex(x => x.ProductId);
    }
}
