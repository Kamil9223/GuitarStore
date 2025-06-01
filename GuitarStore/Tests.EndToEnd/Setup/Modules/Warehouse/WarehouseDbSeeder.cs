using Domain.StronglyTypedIds;
using Warehouse.Core.Database;
using Warehouse.Core.Entities;

namespace Tests.EndToEnd.Setup.Modules.Warehouse;
internal static class WarehouseDbSeeder
{
    public static Stock SeedStock(
        this WarehouseDbContext context,
        ProductId? productId = null,
        int? quantity = null)
    {
        var stock = new Stock
        {
            ProductId = productId ?? ProductId.New(),
            Quantity = quantity ?? 0
        };
        context.Add(stock);
        return stock;
    }
}
