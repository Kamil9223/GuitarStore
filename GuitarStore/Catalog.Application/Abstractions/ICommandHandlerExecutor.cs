using Application;

namespace Catalog.Application.Abstractions;

public interface ICommandHandlerExecutor
{
    Task Execute<TCommand>(TCommand command) where TCommand : ICommand;
}
