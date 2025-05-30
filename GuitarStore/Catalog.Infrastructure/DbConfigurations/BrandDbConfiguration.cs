﻿using Catalog.Domain;
using Catalog.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.DbConfigurations;

internal class BrandDbConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands", CatalogDbContext.Schema);

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
               .HasConversion(
                   id => id!.Value,
                   value => new BrandId(value));

        builder.Property(x => x.Name).HasMaxLength(1000);
    }
}
