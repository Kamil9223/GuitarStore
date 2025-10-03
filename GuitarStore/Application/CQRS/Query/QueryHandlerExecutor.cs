using Microsoft.Extensions.DependencyInjection;

namespace Application.CQRS.Query;

internal class QueryHandlerExecutor : IQueryHandlerExecutor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public QueryHandlerExecutor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<TResponse> Execute<TQuery, TResponse>(TQuery query, CancellationToken ct)
        where TQuery : IQuery
        where TResponse : class
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return await handler.Handle(query, ct);
    }
}
