using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.AcousticGuitars;
using Warehouse.Domain.Store;
using Warehouse.Infrastructure.Database;

namespace Warehouse.Infrastructure.AcousticGuitars;

internal class AcousticGuitarDbConfiguration : IEntityTypeConfiguration<AcousticGuitar>
{
    public void Configure(EntityTypeBuilder<AcousticGuitar> builder)
    {
        builder.ToTable("AcousticGuitars", WarehouseDbContext.DbSchema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne<GuitarStore>()
            .WithMany()
            .HasForeignKey(x => x.GuitarStoreId);

        builder.Property(x => x.CompanyName).HasMaxLength(75).IsRequired();
        builder.Property(x => x.ModelName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Price).IsRequired();
    }
}
