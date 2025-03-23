namespace Application.CQRS;
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task Handle(TCommand command);
}

public interface ICommandHandler<TResponse, TCommand>  where TCommand : ICommand
{
    Task<TResponse> Handle(TCommand command);
}
