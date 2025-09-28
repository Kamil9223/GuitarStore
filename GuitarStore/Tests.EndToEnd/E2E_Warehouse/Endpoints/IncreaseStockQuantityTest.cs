using GuitarStore.Api.Client;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Warehouse;
using Xunit;

namespace Tests.EndToEnd.E2E_Warehouse.Endpoints;
public sealed class IncreaseStockQuantityTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task IncreaseProductQuantity_WhenProductNotExists_NotFoundIsReturned()
    {
        //Assert
        var request = new IncreaseStockQuantityCommand
        {
            Id = Guid.NewGuid(),
            IncreaseBy = 120
        };

        //Act
        var action = () => TestContext.GuitarStoreClient.IncreaseProductQuantityAsync(request);

        //Assert
        var ex = await Should.ThrowAsync<ApiException>(action);
        ex.StatusCode.ShouldBe(404);
    }

    [Fact]
    public async Task IncreaseProductQuantity_WhenProductExists_ShouldSucceed()
    {
        var productOnStock = Databases.WarehouseDbContext.SeedStock(quantity: 10);
        await Databases.WarehouseDbContext.SaveChangesAsync();
        var increaseBy = 5;

        //Assert
        var request = new IncreaseStockQuantityCommand
        {
            Id = productOnStock.ProductId.Value,
            IncreaseBy = increaseBy
        };

        //Act
        await TestContext.GuitarStoreClient.IncreaseProductQuantityAsync(request);
        Databases.WarehouseDbContext.ChangeTracker.Clear();

        //Assert
        var dbStock = await Databases.WarehouseDbContext.Stock.SingleAsync(x => x.ProductId == productOnStock.ProductId);
        dbStock.ShouldNotBeNull();
        dbStock.Quantity.ShouldBe(productOnStock.Quantity + increaseBy);
    }
}
