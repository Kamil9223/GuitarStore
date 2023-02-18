using Application;

namespace Warehouse.Application.Abstractions;

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task Handle(TCommand command);
}
