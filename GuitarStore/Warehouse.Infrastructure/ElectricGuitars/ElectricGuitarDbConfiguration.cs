using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.ElectricGuitars;
using Warehouse.Domain.Store;
using Warehouse.Infrastructure.Database;

namespace Warehouse.Infrastructure.ElectricGuitars;

internal class ElectricGuitarDbConfiguration : IEntityTypeConfiguration<ElectricGuitar>
{
    public void Configure(EntityTypeBuilder<ElectricGuitar> builder)
    {
        builder.ToTable("ElectricGuitars", WarehouseDbContext.DbSchema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.GuitarStore)
            .WithMany(x => x.ElectricGuitars)
            .HasForeignKey(x => x.GuitarStoreId);

        builder.HasMany(x => x.Pickups)
            .WithMany(x => x.ElectricGuitars);

        builder.Property(x => x.CompanyName).HasMaxLength(75).IsRequired();
        builder.Property(x => x.ModelName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Price).HasColumnType("decimal(10,2)").IsRequired();
    }
}
