using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.DbConfigurations;

internal class VariationOptionDbConfiguration : IEntityTypeConfiguration<VariationOption>
{
    public void Configure(EntityTypeBuilder<VariationOption> builder)
    {
        builder.ToTable("VariationOptions", "Catalog");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Value).HasMaxLength(1000);
    }
}
