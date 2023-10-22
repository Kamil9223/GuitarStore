using Customers.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

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

    public async Task<IDbTransaction> BeginTransaction()
    {
        var transaction = await _customersDbContext.Database.BeginTransactionAsync();

        return transaction.GetDbTransaction();
    }
}
