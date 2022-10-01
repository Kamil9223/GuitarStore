using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Store;

namespace Warehouse.Infrastructure.Store;

internal class GuitarStoreRepository : GenericRepository<GuitarStore>, IGuitarStoreRepository
{
    public GuitarStoreRepository(DbContext context) : base(context)
    {
    }


}
