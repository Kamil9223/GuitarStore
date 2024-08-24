using Catalog.Domain;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.DbConfigurations;

internal class ProductDbConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "Catalog");

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
               .HasConversion(
                   id => id!.Value,
                   value => new ProductId(value));

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Price).HasColumnType("decimal(10,2)");

        builder
            .HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId);

        builder
            .HasOne(x => x.Brand)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.BrandId);

        builder
           .HasMany(e => e.VariationOptions)
           .WithMany(e => e.Products)
           .UsingEntity(
               l => l.HasOne(typeof(VariationOption)).WithMany().HasForeignKey("VariationOptionId"),
               r => r.HasOne(typeof(Product)).WithMany().HasForeignKey("ProductId"));
    }
}
