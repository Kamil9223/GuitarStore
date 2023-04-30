using System.Data;

namespace Warehouse.Application.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();
}
