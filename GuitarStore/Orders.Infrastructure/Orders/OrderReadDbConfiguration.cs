using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Orders;
using Orders.Infrastructure.Database;

namespace Orders.Infrastructure.Orders;
internal class OrderReadDbConfiguration : IEntityTypeConfiguration<OrderReadModel>
{
    public void Configure(EntityTypeBuilder<OrderReadModel> builder)
    {
        builder.ToTable("Orders_Read", OrdersDbContext.Schema);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });

        builder.OwnsOne(x => x.DeliveryAddress, b =>
        {
            b.Property(x => x.Country).HasColumnName("Delivery_Country").HasMaxLength(300).IsRequired();
            b.Property(x => x.LocalityName).HasColumnName("Delivery_LocalityName").HasMaxLength(500).IsRequired();
            b.Property(x => x.PostalCode).HasColumnName("Delivery_PostalCode").HasMaxLength(10).IsRequired();
            b.Property(x => x.HouseNumber).HasColumnName("Delivery_HouseNumber").HasMaxLength(20).IsRequired();
            b.Property(x => x.Street).HasColumnName("Delivery_Street").HasMaxLength(500).IsRequired();
            b.Property(x => x.LocalNumber).HasColumnName("Delivery_LocalNumber").HasMaxLength(20).IsRequired(false);
        });

        builder.Property(x => x.Status).HasConversion(
            num => (OrderStatus)num,
            status => (byte)status);
    }
}

internal class OrderReadItemDbConfiguration : IEntityTypeConfiguration<OrderItemReadModel>
{
    public void Configure(EntityTypeBuilder<OrderItemReadModel> builder)
    {
        builder.ToTable("OrderItems_Read", OrdersDbContext.Schema);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OrderId);
    }
}
