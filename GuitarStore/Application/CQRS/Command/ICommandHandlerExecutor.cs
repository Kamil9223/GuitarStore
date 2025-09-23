namespace Application.CQRS.Command;

public interface ICommandHandlerExecutor
{
    Task Execute<TCommand>(TCommand command) where TCommand : ICommand;

    Task<TResponse> Execute<TResponse, TCommand>(TCommand command) where TCommand : ICommand;
}
