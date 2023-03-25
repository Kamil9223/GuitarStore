using Application;
using Warehouse.Application.Abstractions;

namespace Warehouse.Application.CommandQueryExecutors;

internal class QueryHandlerExecutor<TQuery, TResponse> : IQueryHandlerExecutor<TQuery, TResponse>
    where TQuery : IQuery
    where TResponse : class
{
    private readonly IQueryHandler<TQuery, TResponse> _handler;

    public QueryHandlerExecutor(IQueryHandler<TQuery, TResponse> handler)
    {
        _handler = handler;
    }

    public async Task<TResponse> Execute(TQuery query)
    {
        return await _handler.Handle(query);
    }
}
