using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Domain.Store;
using Warehouse.Infrastructure.Database;

namespace Warehouse.Infrastructure.Store;

internal class GuitarStoreRepository : GenericRepository<GuitarStore>, IGuitarStoreRepository
{
    public GuitarStoreRepository(WarehouseDbContext context) : base(context)
    {
    }


}
