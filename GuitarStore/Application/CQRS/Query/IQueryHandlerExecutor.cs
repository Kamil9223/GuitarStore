namespace Application.CQRS.Query;

public interface IQueryHandlerExecutor

{
    Task<TResponse> Execute<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery
        where TResponse : class;
}
