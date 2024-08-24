using Customers.Domain.Customers;
using Customers.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Customers;

internal class CustomerDbConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", CustomersDbContext.Schema);

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id!.Value,
                value => new CustomerId(value));

        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);

        builder.OwnsOne(x => x.Address, builder =>
        {
            builder.Property(x => x.Country).HasColumnName("Country").HasMaxLength(300).IsRequired(false);
            builder.Property(x => x.Locality).HasConversion(
                x => x.ToString(),
                x => (Locality)Enum.Parse(typeof(Locality), x))
                .HasColumnName("Locality").HasMaxLength(15).IsRequired(false);
            builder.Property(x => x.LocalityName).HasColumnName("LocalityName").HasMaxLength(500).IsRequired(false);
            builder.Property(x => x.PostalCode).HasColumnName("PostalCode").HasMaxLength(10).IsRequired(false);
            builder.Property(x => x.HouseNumber).HasColumnName("HouseNumber").HasMaxLength(20).IsRequired(false);
            builder.Property(x => x.Street).HasColumnName("Street").HasMaxLength(500).IsRequired(false);
            builder.Property(x => x.LocalNumber).HasColumnName("LocalNumber").HasMaxLength(20).IsRequired(false);
        });

        builder.OwnsOne(x => x.Email, builder =>
        {
            builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100);
        });
    }
}
