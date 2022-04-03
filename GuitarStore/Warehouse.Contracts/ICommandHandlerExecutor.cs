using Application;

namespace Warehouse.Contracts;

public interface ICommandHandlerExecutor<TCommand> where TCommand : ICommand
{
    Task Execute(TCommand command);
}
