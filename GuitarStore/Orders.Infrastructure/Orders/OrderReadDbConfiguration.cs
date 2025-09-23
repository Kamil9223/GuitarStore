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
        builder.OwnsOne(x => x.DeliveryAddress);
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
