using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Products;
using Orders.Infrastructure.Database;

namespace Orders.Infrastructure.Products;
internal class ProductsDbConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", OrdersDbContext.Schema);

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id!.Value,
                value => new ProductId(value));

        builder.Property(x => x.Name).HasMaxLength(200);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.OwnsOne(x => x.Price, builder =>
        {
            builder.Property(x => x.Value).HasColumnName("Price").HasColumnType("decimal(10,2)");
        });
    }
}
