namespace Application.CQRS.Command;
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken ct);
}

public interface ICommandHandler<TResponse, TCommand> where TCommand : ICommand
{
    Task<TResponse> Handle(TCommand command, CancellationToken ct);
}
