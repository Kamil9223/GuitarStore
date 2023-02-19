using Dapper;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Store;

namespace Warehouse.Infrastructure.Store;

internal class GuitarStoreRepository : GenericRepository<GuitarStore>, IGuitarStoreRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GuitarStoreRepository(DbContext context, ISqlConnectionFactory sqlConnectionFactory) : base(context)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<int> CountOfProductsInStore(int storeId)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = "SELECT COUNT(*) "+
                           "FROM [Warehouse].[Products] AS [P] " +
                           "INNER JOIN [Warehouse].[Stores] AS [S] ON [P].GuitarStoreId = [S].Id " +
                           "WHERE [S].[Id] = @StoreId";

        return connection.QuerySingleAsync<int>(
            sql,
            new
            {
                StoreId = storeId,
            });
    }
}
