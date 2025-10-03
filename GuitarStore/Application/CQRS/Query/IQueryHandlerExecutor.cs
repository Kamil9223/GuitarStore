namespace Application.CQRS.Query;

public interface IQueryHandlerExecutor

{
    Task<TResponse> Execute<TQuery, TResponse>(TQuery query, CancellationToken ct)
        where TQuery : IQuery
        where TResponse : class;
}
