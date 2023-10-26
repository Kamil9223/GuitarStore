using Application.CQRS;

namespace Catalog.Application.Abstractions;

public interface IQueryHandlerExecutor
    
{
    Task<TResponse> Execute<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery
        where TResponse : class;
}
