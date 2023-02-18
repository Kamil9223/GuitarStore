﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Products;

namespace Warehouse.Infrastructure.Products;

internal class ProductDbConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "Warehouse");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.GuitarStore)
            .WithMany(x => x.Products);

        builder.HasOne(x => x.Category)
             .WithMany(x => x.Products);

        builder.Property(x => x.ProducerName).HasMaxLength(75);
        builder.HasIndex(x => x.ProducerName).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(100);

        builder.OwnsOne(x => x.Price, builder =>
        {
            builder.Property(x => x.Value).HasColumnName("Price").HasColumnType("decimal(10,2)");
        });     
    }
}
