using Catalog.Domain;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.DbConfigurations;

internal class VariationDbConfiguration : IEntityTypeConfiguration<Variation>
{
    public void Configure(EntityTypeBuilder<Variation> builder)
    {
        builder.ToTable("Variations", "Catalog");

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
               .HasConversion(
                   id => id!.Value,
                   value => new VariationId(value));

        builder.Property(x => x.Name).HasMaxLength(1000);

        builder.HasMany(x => x.VariationOptions)
           .WithOne(x => x.Variation);
    }
}
