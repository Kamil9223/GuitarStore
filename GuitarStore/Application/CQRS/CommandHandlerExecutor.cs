﻿using Autofac;

namespace Application.CQRS;

internal class CommandHandlerExecutor : ICommandHandlerExecutor
{
    private readonly ILifetimeScope _scope;

    public CommandHandlerExecutor(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public async Task Execute<TCommand>(TCommand command) where TCommand : ICommand
    {
        using var scope = _scope.BeginLifetimeScope();
        var handler = scope.Resolve<ICommandHandler<TCommand>>();
        await handler.Handle(command);
    }
}
