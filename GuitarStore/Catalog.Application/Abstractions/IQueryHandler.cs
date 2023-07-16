using Application;

namespace Catalog.Application.Abstractions;

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery
    where TResponse : class
{
    Task<TResponse> Handle(TQuery query);
}
