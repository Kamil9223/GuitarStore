using Customers.Domain.Carts;
using GuitarStore.Api.Client;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Customers;
using Xunit;

namespace Tests.EndToEnd.E2E_Customers.Endpoints;
public sealed class AddItemToCartTest(Setup.Application app) : EndToEndTestBase(app)
{
    //TODO: just sample test for check wheter database works. Will be refactored (domain logic also)
    [Fact]
    public async Task AddItemToCartTest_WhenProductExists_()
    {
        // Arrange
        var customer = Databases.CustomersDbContext.SeedCustomer();
        Databases.CustomersDbContext.SeedCart(customer.Id);
        var product = Databases.CustomersDbContext.SeedProduct();
        await Databases.CustomersDbContext.SaveChangesAsync();

        var request = new AddCartItemCommand
        {
            CustomerId = customer.Id.Value,
            Name = product.Name,
            Price = (double)product.Price.Value,//TODO: Fix API generation (now decimal is treat as double)
            ProductId = product.Id.Value,
        };

        //Act
        await TestContext.GuitarStoreClient.CartsAsync(request);

        //Assert
        Databases.CustomersDbContext.ChangeTracker.Clear();
        var cart = await Databases.CustomersDbContext.Carts.SingleOrDefaultAsync(x => x.CustomerId == customer.Id);
        cart.ShouldNotBeNull();
        cart.CartState.ShouldBe(CartState.ContainingProducts);
        var entityCart = JsonConvert.DeserializeObject<Cart>(cart.Object);
        entityCart.ShouldNotBeNull();
        entityCart.CartItems.Count.ShouldBe(1);
        entityCart.CartItems.ShouldContain(x => x.ProductId == product.Id);
    }
}
