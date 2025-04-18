﻿using Catalog.Domain;
using Catalog.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.DbConfigurations;

internal class VariationOptionDbConfiguration : IEntityTypeConfiguration<VariationOption>
{
    public void Configure(EntityTypeBuilder<VariationOption> builder)
    {
        builder.ToTable("VariationOptions", CatalogDbContext.Schema);

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
               .HasConversion(
                   id => id!.Value,
                   value => new VariationOptionId(value));

        builder.Property(x => x.Value).HasMaxLength(1000);
    }
}
