using System.Data;

namespace Orders.Application.Abstractions;
public interface IUnitOfWork
{
    Task SaveChanges();

    Task<IDbTransaction> BeginTransaction();
}
