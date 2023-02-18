using Application;
using Warehouse.Application.Abstractions;
using Warehouse.Application.AppMIddlewareServices;

namespace Warehouse.Application.CommandQueryExecutors;

internal class CommandHandlerExecutor<TCommand> : ICommandHandlerExecutor<TCommand> where TCommand : ICommand
{
    private readonly IValidationService<TCommand> _validationService;
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IUnitOfWorkService _unitOfWorkService;

    public CommandHandlerExecutor(
        IValidationService<TCommand> validationService,
        ICommandHandler<TCommand> handler,
        IUnitOfWorkService unitOfWorkService)
    {
        _validationService = validationService;
        _handler = handler;
        _unitOfWorkService = unitOfWorkService;
    }

    public async Task Execute(TCommand command)
    {
        _validationService.Validate(command);

        await _handler.Handle(command);

        await _unitOfWorkService.Commit();
    }
}
