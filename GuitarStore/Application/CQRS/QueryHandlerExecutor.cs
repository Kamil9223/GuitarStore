using Microsoft.Extensions.DependencyInjection;

namespace Application.CQRS;

internal class QueryHandlerExecutor : IQueryHandlerExecutor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public QueryHandlerExecutor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<TResponse> Execute<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery
        where TResponse : class
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return await handler.Handle(query);
    }
}
