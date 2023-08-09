using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.DbConfigurations;

internal class ProductDbConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "Catalog");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.HasIndex(x => x.Name).IsUnique();

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
