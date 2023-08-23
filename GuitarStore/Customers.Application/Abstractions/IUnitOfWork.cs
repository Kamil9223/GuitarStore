namespace Customers.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChanges();
}
