using Domain.StronglyTypedIds;
using GuitarStore.Api.Client;
using Orders.Domain.Orders;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Orders;
using Xunit;

namespace Tests.EndToEnd.E2E_Orders.Endpoints;
public sealed class CheckOrderStatusTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task CheckOrderStatus_WhenOrderExists_ShouldSucceed()
    {
        //Arrange
        var customer = Databases.OrdersDbContext.SeedCustomer(CustomerId.New());
        var readOrderModel = Databases.OrdersDbContext.SeedOrderReadModel(customer.Id, status: OrderStatus.Realized);
        await Databases.OrdersDbContext.SaveChangesAsync();

        //Act
        var response = await TestContext.GuitarStoreClient.CheckOrderStatusAsync(readOrderModel.Id);

        //Assert
        response.Status.ShouldBe((byte)OrderStatus.Realized);
    }

    [Fact]
    public async Task CheckOrderStatus_WhenOrderDoesNotExists_NotFoundIsReturned()
    {
        //Act
        var action = () => TestContext.GuitarStoreClient.CheckOrderStatusAsync(Guid.NewGuid());

        //Assert
        var ex = await Should.ThrowAsync<ApiException>(action);
        ex.StatusCode.ShouldBe(404);
    }
}
