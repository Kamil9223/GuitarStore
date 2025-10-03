using Common.Errors.Exceptions;
using Customers.Domain.Customers;
using Customers.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Customers.Infrastructure.Customers;

internal class CustomerRepository : ICustomerRepository
{
    private readonly CustomersDbContext _customersDbContext;

    public CustomerRepository(CustomersDbContext customersDbContext)
    {
        _customersDbContext = customersDbContext;
    }

    public void Add(Customer customer)
    {
        _customersDbContext.Customers.Add(customer);
    }

    public async Task<bool> Exists(Expression<Func<Customer, bool>> predicate, CancellationToken ct)
    {
        return await _customersDbContext.Customers.AnyAsync(predicate, ct);
    }

    public async Task<Customer> Get(CustomerId id, CancellationToken ct) =>
        await _customersDbContext.Customers.SingleOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException(id);
}
