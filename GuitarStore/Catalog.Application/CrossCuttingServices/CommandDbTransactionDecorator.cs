using Application;
using Catalog.Application.Abstractions;

namespace Catalog.Application.CrossCuttingServices;

internal class CommandDbTransactionDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IUnitOfWork _unitOfWork;

    public CommandDbTransactionDecorator(ICommandHandler<TCommand> handler, IUnitOfWork unitOfWork)
    {
        _handler = handler;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TCommand command)
    {
        using var dbTransaction = await _unitOfWork.BeginTransaction();

        await _handler.Handle(command);

        dbTransaction.Commit();
    }
}
