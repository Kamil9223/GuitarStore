namespace Application.CQRS.Query;
public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery
    where TResponse : class
{
    Task<TResponse> Handle(TQuery query, CancellationToken ct);
}
