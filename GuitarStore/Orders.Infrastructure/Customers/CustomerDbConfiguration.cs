using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Customers;

namespace Orders.Infrastructure.Customers;
internal class CustomerDbConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);

        builder.OwnsOne(x => x.Email, builder =>
        {
            builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100);
        });
    }
}
