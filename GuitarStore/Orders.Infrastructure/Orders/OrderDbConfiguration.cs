using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Customers;

namespace Orders.Infrastructure.Orders;
internal class OrderDbConfiguration : IEntityTypeConfiguration<OrderDbModel>
{
    public void Configure(EntityTypeBuilder<OrderDbModel> builder)
    {
        builder.ToTable("Orders", "Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Object).IsRequired();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId);
    }
}
