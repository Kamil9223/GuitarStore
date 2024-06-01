namespace Orders.Application.Instructions;
public interface IInstructionService
{
    Task RegisterInstruction(InstructionType instructionType);
}

public enum InstructionType
{
    Products_Synchronization = 0
}
