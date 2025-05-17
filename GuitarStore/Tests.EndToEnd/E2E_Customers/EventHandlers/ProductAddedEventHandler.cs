using Customers.Application.Products.Events.Incoming;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Customers;
using Tests.EndToEnd.Setup.TestsHelpers;
using Xunit;

namespace Tests.EndToEnd.E2E_Customers.EventHandlers;
public sealed class ProductAddedEventHandler(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task Handle_WhenProductDoesNotExists_ProductIsAdded()
    {
        var product = new ProductAddedEvent(
            Id: ProductId.New(),
            Name: Guid.NewGuid().ToString(),
            Price: 99.99M,
            Quantity: 100);

        RabbitMqChannel.PublishTestEvent(product);

        await Waiter.WaitForCondition(async () =>
        {
            var newProduct = await Databases.CustomersDbContext.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            return newProduct is not null;
        }, TimeSpan.FromSeconds(1));

        var insertedProduct = await Databases.CustomersDbContext.Products
            .SingleAsync(x => x.Id == product.Id);

        insertedProduct.Name.ShouldBe(product.Name);
        insertedProduct.Price.Value.ShouldBe(product.Price);
        insertedProduct.Quantity.ShouldBe(product.Quantity);
    }

    [Fact]
    public async Task Handle_WhenProductExists_ProductQuantityIsIncreased()
    {
        var product = Databases.CustomersDbContext.SeedProduct();
        await Databases.CustomersDbContext.SaveChangesAsync();

        var productEvent = new ProductAddedEvent(
            Id: product.Id,
            Name: product.Name,
            Price: product.Price,
            Quantity: 100);

        RabbitMqChannel.PublishTestEvent(productEvent);

        await Waiter.WaitForCondition(async () =>
        {
            Databases.CustomersDbContext.ChangeTracker.Clear();

            var newProduct = await Databases.CustomersDbContext.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            return newProduct!.Quantity == product.Quantity + productEvent.Quantity;
        }, TimeSpan.FromSeconds(1));


        var insertedProduct = await Databases.CustomersDbContext.Products
            .SingleAsync(x => x.Id == product.Id);

        insertedProduct.Quantity.ShouldBe(product.Quantity + productEvent.Quantity);
    }
}
