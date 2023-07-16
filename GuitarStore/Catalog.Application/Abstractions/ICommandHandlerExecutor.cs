using Application;

namespace Catalog.Application.Abstractions;

public interface ICommandHandlerExecutor<TCommand> where TCommand : ICommand
{
    Task Execute(TCommand command);
}
