using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

        builder.HasOne<GuitarStore>()
            .WithMany()
            .HasForeignKey(x => x.GuitarStoreId);

        builder.Property(x => x.CompanyName).HasMaxLength(75).IsRequired();
        builder.Property(x => x.ModelName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Price).IsRequired();

        builder.OwnsMany<Pickup>("Pickups", builder =>
        {
            builder.WithOwner().HasForeignKey(x => x.ElectricGuitarId);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PickupType)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<PickupType>());
        });
    }
}
