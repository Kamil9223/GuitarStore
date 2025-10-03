using Domain.StronglyTypedIds;
using System.Linq.Expressions;

namespace Customers.Domain.Customers;

public interface ICustomerRepository
{
    void Add(Customer customer);
    Task<bool> Exists(Expression<Func<Customer, bool>> predicate, CancellationToken ct);
    Task<Customer> Get(CustomerId id, CancellationToken ct);
}
