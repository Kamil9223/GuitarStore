﻿using Domain;

namespace Warehouse.Domain.Store;

public interface IGuitarStoreRepository : IRepository<GuitarStore>
{
    Task<int> CountOfProductsInStore(int storeId);
}
