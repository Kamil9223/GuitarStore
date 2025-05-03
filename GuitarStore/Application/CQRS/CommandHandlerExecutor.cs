using Microsoft.Extensions.DependencyInjection;

namespace Application.CQRS;

internal class CommandHandlerExecutor : ICommandHandlerExecutor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CommandHandlerExecutor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute<TCommand>(TCommand command) where TCommand : ICommand
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.Handle(command);
    }

    public async Task<TResponse> Execute<TResponse, TCommand>(TCommand command) where TCommand : ICommand
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TResponse, TCommand>>();
        return await handler.Handle(command);
    }
}
