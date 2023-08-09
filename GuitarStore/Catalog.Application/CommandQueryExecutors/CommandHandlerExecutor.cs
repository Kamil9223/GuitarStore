using Application;
using Catalog.Application.Abstractions;

namespace Catalog.Application.CommandQueryExecutors;

internal class CommandHandlerExecutor<TCommand> : ICommandHandlerExecutor<TCommand> where TCommand : ICommand
{
    private readonly IValidationService<TCommand> _validationService;
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IUnitOfWork _unitOfWork;

    public CommandHandlerExecutor(
        IValidationService<TCommand> validationService,
        ICommandHandler<TCommand> handler,
        IUnitOfWork unitOfWork)
    {
        _validationService = validationService;
        _handler = handler;
        _unitOfWork = unitOfWork;
    }

    public async Task Execute(TCommand command)
    {
        _validationService.Validate(command);

        using var dbTransaction = await _unitOfWork.BeginTransaction();

        await _handler.Handle(command);

        dbTransaction.Commit();
    }
}
