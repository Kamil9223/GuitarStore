using Microsoft.EntityFrameworkCore;
using Orders.Application.Instructions;
using Orders.Infrastructure.Database;

namespace Orders.Infrastructure.Instructions;
internal class InstructionService : IInstructionService
{
    private readonly OrdersDbContext _context;

    public InstructionService(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task RegisterInstruction(InstructionType instructionType)
    {
        var isExist = await _context.Instructions
            .Where(x => x.IsRealized == false)
            .Where(x => x.InstructionType == instructionType)
            .AnyAsync();

        if (isExist)
            return;

        _context.Instructions.Add(new Instruction
        {
            InstructionType = instructionType,
            IsRealized = false,
            RegisteredAt = DateTimeOffset.Now
        });

        await _context.SaveChangesAsync();
    }
}
