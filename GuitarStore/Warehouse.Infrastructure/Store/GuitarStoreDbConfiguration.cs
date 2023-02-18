using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Store;

namespace Warehouse.Infrastructure.Store;

internal class GuitarStoreDbConfiguration : IEntityTypeConfiguration<GuitarStore>
{
    public void Configure(EntityTypeBuilder<GuitarStore> builder)
    {
        builder.ToTable("Stores", "Warehouse");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasMaxLength(30);
        
        builder.Property(x => x.City).HasMaxLength(200);
        builder.Property(x => x.Street).HasMaxLength(400);
        builder.Property(x => x.PostalCode).HasMaxLength(20);
    }
}
