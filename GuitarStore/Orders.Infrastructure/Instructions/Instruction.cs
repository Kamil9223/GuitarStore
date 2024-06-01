using Orders.Application.Instructions;

namespace Orders.Infrastructure.Instructions;
public class Instruction
{
    public int Id { get; }
    public InstructionType InstructionType { get; init; }
    public bool IsRealized { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
    public DateTimeOffset? RealizedAt { get; init; }
}
