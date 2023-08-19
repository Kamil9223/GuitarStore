using Application;
using Catalog.Application.Abstractions;

namespace Catalog.Application.CrossCuttingServices;

internal class CommandValidationDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IValidationService<TCommand> _validationService;

    public CommandValidationDecorator(ICommandHandler<TCommand> handler, IValidationService<TCommand> validationService)
    {
        _handler = handler;
        _validationService = validationService;
    }

    public async Task Handle(TCommand command)
    {
        _validationService.Validate(command);

        await _handler.Handle(command);
    }
}
