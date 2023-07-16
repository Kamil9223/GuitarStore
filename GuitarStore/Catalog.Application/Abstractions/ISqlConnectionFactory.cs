using System.Data;

namespace Catalog.Application.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();
}
