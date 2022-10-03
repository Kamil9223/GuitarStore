using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Store;
using Warehouse.Infrastructure.Database;

namespace Warehouse.Infrastructure.Store;

internal class GuitarStoreDbConfiguration : IEntityTypeConfiguration<GuitarStore>
{
    public void Configure(EntityTypeBuilder<GuitarStore> builder)
    {
        builder.ToTable(WarehouseDbContext.GuitarStoreTableName, WarehouseDbContext.DbSchema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasMaxLength(30).IsRequired();
        
        builder.OwnsOne<StoreLocation>("Location", builder =>
        {
            builder.Property(x => x.City).HasColumnName("City").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Street).HasColumnName("Street").HasMaxLength(400).IsRequired();
            builder.Property(x => x.PostalCode).HasColumnName("PostalCode").HasColumnType("char(6)").IsRequired();
        });
    }
}
