using Catalog.Application.Abstractions;
using Common.EfCore.Transactions;

namespace Catalog.Infrastructure.Database;
internal class CatalogUnitOfWork(CatalogDbContext dbContext)
    : UnitOfWork<CatalogDbContext>(dbContext), ICatalogUnitOfWork
{
}
