using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Orders.Infrastructure.Instructions;
internal class InstructionConfiguration : IEntityTypeConfiguration<Instruction>
{
    public void Configure(EntityTypeBuilder<Instruction> builder)
    {
        builder.ToTable("Instructions", "Orders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.InstructionType)
            .HasConversion(
                x => x.ToString(),
                x => (InstructionType)Enum.Parse(typeof(InstructionType), x))
            .HasMaxLength(100);
    }
}
