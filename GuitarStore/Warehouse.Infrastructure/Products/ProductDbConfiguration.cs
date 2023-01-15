using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Product;
using Warehouse.Infrastructure.Database;

namespace Warehouse.Infrastructure.Products;

internal class ProductDbConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(WarehouseDbContext.ProductTableName, WarehouseDbContext.DbSchema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.GuitarStore)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.GuitarStoreId);

        builder.HasOne(x => x.Category)
             .WithMany(x => x.Products)
             .HasForeignKey(x => x.CategoryId);

        builder.OwnsOne(x => x.ProductModel, builder =>
        {
            builder.Property(x => x.ProducerName).HasColumnName("ProducerName").HasMaxLength(75).IsRequired();
            builder.HasIndex(x => x.ProducerName).IsUnique();
            builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(x => x.Price, builder =>
        {
            builder.Property(x => x.Currency).HasColumnName("Currency").HasColumnType("Char(3)").IsRequired();
            builder.Property(x => x.Value).HasColumnName("Price").HasColumnType("decimal(10,2)").IsRequired();
        });     
        
        builder.Property(x => x.Description).IsRequired();
    }
}
