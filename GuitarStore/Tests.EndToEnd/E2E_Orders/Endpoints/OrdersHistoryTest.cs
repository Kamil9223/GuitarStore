using Domain.StronglyTypedIds;
using Orders.Domain.Orders;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Orders;
using Xunit;

namespace Tests.EndToEnd.E2E_Orders.Endpoints;
public sealed class OrdersHistoryTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task GetOrdersHistory_WhenCustomerHasntPlaceOrderYet_EmptyListReturned()
    {
        //Act
        var response = await TestContext.GuitarStoreClient.OrderHistoryAsync(Guid.NewGuid());

        //Assert
        response.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetOrdersHistory_WhenCustomerHasSomeFinishedOrders_HistoryIsReturned()
    {
        //Arrange
        var customer = Databases.OrdersDbContext.SeedCustomer(CustomerId.New());
        Databases.OrdersDbContext.SeedOrderReadModel(customer.Id, status: OrderStatus.Realized);
        Databases.OrdersDbContext.SeedOrderReadModel(customer.Id, status: OrderStatus.New);
        Databases.OrdersDbContext.SeedOrderReadModel(customer.Id, status: OrderStatus.Canceled);
        await Databases.OrdersDbContext.SaveChangesAsync();

        //Act
        var response = await TestContext.GuitarStoreClient.OrderHistoryAsync(customer.Id.Value);

        //Assert
        response.Items.Count.ShouldBe(3);
    }
}
