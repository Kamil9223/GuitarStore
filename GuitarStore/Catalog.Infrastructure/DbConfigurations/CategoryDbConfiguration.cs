﻿using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.DbConfigurations;

internal class CategoryDbConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", "Catalog");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryName).HasMaxLength(75);

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
           .HasMany(e => e.Variations)
           .WithMany(e => e.Categories)
           .UsingEntity(
               l => l.HasOne(typeof(Variation)).WithMany().HasForeignKey("VariationId"),
               r => r.HasOne(typeof(Category)).WithMany().HasForeignKey("CategoryId"));
    }
}
