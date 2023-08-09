using System.Data;

namespace Catalog.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChanges();
    Task<IDbTransaction> BeginTransaction();
}
