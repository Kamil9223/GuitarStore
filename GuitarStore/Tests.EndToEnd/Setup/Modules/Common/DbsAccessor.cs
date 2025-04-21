using Catalog.Infrastructure.Database;
using Customers.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Database;
using Warehouse.Core.Database;

namespace Tests.EndToEnd.Setup.Modules.Common;
public class DbsAccessor(IServiceProvider serviceProvider)
{
    internal CatalogDbContext CatalogDbContext => serviceProvider.GetRequiredService<CatalogDbContext>();

    internal CustomersDbContext CustomersDbContext => serviceProvider.GetRequiredService<CustomersDbContext>();

    internal OrdersDbContext OrdersDbContext => serviceProvider.GetRequiredService<OrdersDbContext>();

    internal WarehouseDbContext WarehouseDbContext => serviceProvider.GetRequiredService<WarehouseDbContext>();
}
