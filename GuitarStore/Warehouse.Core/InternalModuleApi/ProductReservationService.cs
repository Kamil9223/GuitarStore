using Domain.Exceptions;
using Warehouse.Core.Database;
using Warehouse.Core.Entities;
using Warehouse.Shared;

namespace Warehouse.Core.InternalModuleApi;
internal class ProductReservationService : IProductReservationService
{
    private readonly WarehouseDbContext _dbContext;

    public ProductReservationService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ReserveProduct(ReserveProductsDto dto)
    {
        var productIds = dto.Products.Select(x => x.ProductId).ToList();
        var productsOnStock = _dbContext.Stock.Where(x => productIds.Contains(x.ProductId)).ToList();

        var missingProducts = productIds.Except(productsOnStock.Select(x => x.ProductId));
        if (missingProducts.Any())
            throw new DomainException($"Critical! Products with ids: [{string.Join(", ", missingProducts)}] does not exists on Stock!");

        foreach (var product in dto.Products)
        {
            var stockProduct = productsOnStock.Single(x => x.ProductId == product.ProductId);
            if (product.Quantity > stockProduct.Quantity)
                throw new DomainException("The requested quantity exceeds the available stock.");

            stockProduct.Quantity -= product.Quantity;
            _dbContext.ProductReservations.Add(new ProductReservation
            {
                OrderId = dto.OrderId,
                ProductId = product.ProductId,
                ReservedQuantity = product.Quantity
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}
//dodac w infraatructure generyczny transactionWrapper
//oraz jak sugeruje gpt taki wraper na cross modulowe transakcje
//wynieść ten decorator do pojedynczego contextu do libki
//unitOfWork -> tylko abstrakcja na kontext żeby robić SaveChanges
//rejestrować te dekoratory per konkretny typ tam gdzie trzeba po prostu