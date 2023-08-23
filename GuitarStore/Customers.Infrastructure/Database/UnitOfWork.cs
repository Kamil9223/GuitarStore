using Customers.Application.Abstractions;

namespace Customers.Infrastructure.Database;

internal class UnitOfWork : IUnitOfWork
{
    private readonly CustomersDbContext _customersDbContext;

    public UnitOfWork(CustomersDbContext customersDbContext)
    {
        _customersDbContext = customersDbContext;
    }

    public async Task SaveChanges()
    {
        await _customersDbContext.SaveChangesAsync();
    }
}
