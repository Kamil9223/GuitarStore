using Application;
using Warehouse.Contracts;

namespace Warehouse.Application;

internal class CommandHandlerExecutor<TCommand> : ICommandHandlerExecutor<TCommand> where TCommand : ICommand
{
    private readonly IValidationService<TCommand> _validationService;
    private readonly ICommandHandler<TCommand> _handler;

    public CommandHandlerExecutor(
        IValidationService<TCommand> validationService,
        ICommandHandler<TCommand> handler)
    {
        _validationService = validationService;
        _handler = handler;
    }

    public async Task Execute(TCommand command)
    {
        _validationService.Validate(command);

        await _handler.Handle(command);
    }
}
