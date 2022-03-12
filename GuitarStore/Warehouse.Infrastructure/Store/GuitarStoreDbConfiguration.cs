using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Store;
using Warehouse.Infrastructure.Database;

namespace Warehouse.Infrastructure.Store;

internal class GuitarStoreDbConfiguration : IEntityTypeConfiguration<GuitarStore>
{
    public void Configure(EntityTypeBuilder<GuitarStore> builder)
    {
        builder.ToTable("Stores", WarehouseDbContext.DbSchema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasMaxLength(30).IsRequired();
        
        builder.OwnsOne<StoreLocation>("Location", builder =>
        {
            builder.Property(x => x.City).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Street).HasMaxLength(400).IsRequired();
            builder.Property(x => x.PostalCode).HasColumnType("char(6)").IsRequired();
        });
    }
}
