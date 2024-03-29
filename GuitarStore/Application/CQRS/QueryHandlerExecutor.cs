﻿using Autofac;

namespace Application.CQRS;

internal class QueryHandlerExecutor : IQueryHandlerExecutor
{
    private readonly ILifetimeScope _scope;

    public QueryHandlerExecutor(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public async Task<TResponse> Execute<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery
        where TResponse : class
    {
        using var scope = _scope.BeginLifetimeScope();
        var handler = scope.Resolve<IQueryHandler<TQuery, TResponse>>();
        return await handler.Handle(query);
    }
}
