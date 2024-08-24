using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Customers;
using Orders.Infrastructure.Database;

namespace Orders.Infrastructure.Orders;
internal class OrderDbConfiguration : IEntityTypeConfiguration<OrderDbModel>
{
    public void Configure(EntityTypeBuilder<OrderDbModel> builder)
    {
        builder.ToTable("Orders", OrdersDbContext.Schema);

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
               .HasConversion(
                   id => id!.Value,
                   value => new OrderId(value));

        builder.Property(x => x.Object).IsRequired();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId);
    }
}
