using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Catalog.Infrastructure.Persistence;

internal class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _catalogDbContext;

    public UnitOfWork(CatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task SaveChanges()
    {
        await _catalogDbContext.SaveChangesAsync();
    }

    public async Task<IDbTransaction> BeginTransaction()
    {
        var transaction = await _catalogDbContext.Database.BeginTransactionAsync();

        return transaction.GetDbTransaction();
    }
}
