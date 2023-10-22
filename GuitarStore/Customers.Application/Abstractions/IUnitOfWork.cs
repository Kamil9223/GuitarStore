using System.Data;

namespace Customers.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChanges();
    Task<IDbTransaction> BeginTransaction();
}
