﻿using Customers.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Products;

internal class ProductDbConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "Customers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(200);
        builder.Property(x => x.Quantity).HasColumnName("Quantity");
        builder.OwnsOne(x => x.Price, builder =>
        {
            builder.Property(x => x.Value).HasColumnName("Price").HasColumnType("decimal(10,2)");
        });
    }
}
