using Customers.Domain.Carts;
using Domain.StronglyTypedIds;
using GuitarStore.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Customers;
using Tests.EndToEnd.Setup.Modules.Orders;
using Tests.EndToEnd.Setup.Modules.Payments;
using Tests.EndToEnd.Setup.Modules.Warehouse;
using Xunit;

namespace Tests.EndToEnd.E2E_Orders.Endpoints;
public sealed class PlaceOrderTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task PlaceOrder_HappyPath_ShouldSucceed()
    {
        //Arrange
        var product = Databases.CustomersDbContext.SeedProduct();
        var address = Databases.CustomersDbContext.SeedAddress();
        var customer = Databases.CustomersDbContext.SeedCustomer(customerAddress: address);
        var cartDbModel = Databases.CustomersDbContext.SeedCheckoutCart(
            customerId: customer.Id,
            items: [
                new CartItem
                (
                    id: CartItemId.New(),
                    productId: product.Id,
                    name: product.Name,
                    price: product.Price,
                    quantity: product.Quantity
                )],
            delivery: new Customers.Domain.Carts.Delivery(DelivererId.New(), Guid.NewGuid().ToString()));
        await Databases.CustomersDbContext.SaveChangesAsync();

        var stock = Databases.WarehouseDbContext.SeedStock(product.Id, product.Quantity);
        await Databases.WarehouseDbContext.SaveChangesAsync();

        Databases.OrdersDbContext.SeedCustomer(customer.Id);
        await Databases.OrdersDbContext.SaveChangesAsync();

        var placeOrderCommand = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            ProvideDeliveryAddress = false
        };

        //Act
        await TestContext.GuitarStoreClient.PlaceOrderAsync(placeOrderCommand);

        //Assert
        var order = Databases.OrdersDbContext.Orders.FirstOrDefault(x => x.CustomerId == customer.Id);
        order.ShouldNotBeNull();
        var productReservation = Databases.WarehouseDbContext.ProductReservations
            .FirstOrDefault(x => x.OrderId == order.Id && x.ProductId == product.Id);
        productReservation.ShouldNotBeNull();
    }

    [Fact]
    public async Task PlaceOrder_WhenProductQuantityExceedsTheAvailableStock_ConflictIsReturned()
    {
        //Arrange
        var product = Databases.CustomersDbContext.SeedProduct();
        var address = Databases.CustomersDbContext.SeedAddress();
        var customer = Databases.CustomersDbContext.SeedCustomer(customerAddress: address);
        var cartDbModel = Databases.CustomersDbContext.SeedCheckoutCart(
            customerId: customer.Id,
            items: [
                new CartItem
                (
                    id: CartItemId.New(),
                    productId: product.Id,
                    name: product.Name,
                    price: product.Price,
                    quantity: product.Quantity
                )],
            delivery: new Customers.Domain.Carts.Delivery(DelivererId.New(), Guid.NewGuid().ToString()));
        await Databases.CustomersDbContext.SaveChangesAsync();

        var stock = Databases.WarehouseDbContext.SeedStock(product.Id, product.Quantity - 1);
        await Databases.WarehouseDbContext.SaveChangesAsync();

        Databases.OrdersDbContext.SeedCustomer(customer.Id);
        await Databases.OrdersDbContext.SaveChangesAsync();

        var placeOrderCommand = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            ProvideDeliveryAddress = false
        };

        //Act
        var action = () => TestContext.GuitarStoreClient.PlaceOrderAsync(placeOrderCommand);

        //Assert
        var exception = await action.ShouldThrowAsync<ApiException>();
        var response = exception.ToFailedResponse();
        response.Status.ShouldBe((int)HttpStatusCode.Conflict);

        var order = Databases.OrdersDbContext.Orders.FirstOrDefault(x => x.CustomerId == customer.Id);
        order.ShouldBeNull();
    }

    [Fact]
    public async Task PlaceOrder_WhenStripeFails_ConflictIsReturned()
    {
        //Arrange
        var product = Databases.CustomersDbContext.SeedProduct();
        var address = Databases.CustomersDbContext.SeedAddress();
        var customer = Databases.CustomersDbContext.SeedCustomer(customerAddress: address);
        var cartDbModel = Databases.CustomersDbContext.SeedCheckoutCart(
            customerId: customer.Id,
            items: [
                new CartItem
                (
                    id: CartItemId.New(),
                    productId: product.Id,
                    name: product.Name,
                    price: product.Price,
                    quantity: product.Quantity
                )],
            delivery: new Customers.Domain.Carts.Delivery(DelivererId.New(), Guid.NewGuid().ToString()));
        await Databases.CustomersDbContext.SaveChangesAsync();

        var stock = Databases.WarehouseDbContext.SeedStock(product.Id, product.Quantity);
        await Databases.WarehouseDbContext.SaveChangesAsync();

        Databases.OrdersDbContext.SeedCustomer(customer.Id);
        await Databases.OrdersDbContext.SaveChangesAsync();

        var placeOrderCommand = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            ProvideDeliveryAddress = false
        };

        var testStripeService = Scope.ServiceProvider.GetRequiredService<TestStripeService>();
        var behaviorKey = Guid.NewGuid();
        testStripeService.AddCheckoutSessionBehavior(behaviorKey, () => throw new Exception("TEST"));
        testStripeService.CheckoutSessionBehaviorKey = behaviorKey;

        //Act
        var action = () => TestContext.GuitarStoreClient.PlaceOrderAsync(placeOrderCommand);

        //Assert
        var exception = await action.ShouldThrowAsync<ApiException>();
        var response = exception.ToFailedResponse();
        response.Status.ShouldBe((int)HttpStatusCode.InternalServerError);

        testStripeService.CheckoutSessionBehaviorKey = Guid.Empty;

        var order = Databases.OrdersDbContext.Orders.FirstOrDefault(x => x.CustomerId == customer.Id);
        order.ShouldBeNull();
        var productReservation = Databases.WarehouseDbContext.ProductReservations.FirstOrDefault(x =>x.ProductId == product.Id);
        productReservation.ShouldBeNull();
    }
}
