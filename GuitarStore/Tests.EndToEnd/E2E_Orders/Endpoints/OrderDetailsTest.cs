using Domain.StronglyTypedIds;
using Orders.Domain.Orders;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Orders;
using Xunit;

namespace Tests.EndToEnd.E2E_Orders.Endpoints;
public sealed class OrderDetailsTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task GetOrderDetails_WhenOrderExists_ShouldSucceed()
    {
        //Arrange
        var customer = Databases.OrdersDbContext.SeedCustomer(CustomerId.New());
        var readOrderModel = Databases.OrdersDbContext.SeedOrderReadModel(customer.Id, status: OrderStatus.Realized);
        await Databases.OrdersDbContext.SaveChangesAsync();

        //Act
        var response = await TestContext.GuitarStoreClient.OrderDetailsAsync(readOrderModel.Id);

        //Assert
        response.CustomerId.ShouldBe(readOrderModel.CustomerId);
        response.CreatedAt.DateTime.ShouldBe(readOrderModel.CreatedAt);
        response.Deliverer.ShouldBe(readOrderModel.Deliverer);
        response.UpdatedAt.DateTime.ShouldBe(readOrderModel.UpdatedAt);
        response.TotalPrice.ShouldBe(readOrderModel.TotalPrice);
        response.ItemsCount.ShouldBe(readOrderModel.ItemsCount);
        response.Status.ShouldBe(readOrderModel.Status);
        response.DeliveryAddress.LocalNumber.ShouldBe(readOrderModel.DeliveryAddress.LocalNumber);
        response.DeliveryAddress.HouseNumber.ShouldBe(readOrderModel.DeliveryAddress.HouseNumber);
        response.DeliveryAddress.Country.ShouldBe(readOrderModel.DeliveryAddress.Country);
        response.DeliveryAddress.PostalCode.ShouldBe(readOrderModel.DeliveryAddress.PostalCode);
        response.DeliveryAddress.Street.ShouldBe(readOrderModel.DeliveryAddress.Street);
        response.DeliveryAddress.LocalityName.ShouldBe(readOrderModel.DeliveryAddress.LocalityName);
        response.Items.ShouldBeEmpty();
    }
}
