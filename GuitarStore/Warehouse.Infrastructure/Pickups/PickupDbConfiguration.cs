using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Warehouse.Domain.ElectricGuitars;
using Warehouse.Infrastructure.Database;

namespace Warehouse.Infrastructure.Pickups;

internal class PickupDbConfiguration : IEntityTypeConfiguration<Pickup>
{
    public void Configure(EntityTypeBuilder<Pickup> builder)
    {
        builder.ToTable("Pickups", WarehouseDbContext.DbSchema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PickupType)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<PickupType>())
            .HasMaxLength(30);
    }
}
