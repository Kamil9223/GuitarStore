using System.Data;

namespace Warehouse.Application.DataAccessAbstraction;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();
}
