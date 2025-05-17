using Catalog.Application.Products.Events.Outgoing;
using GuitarStore.Api.Client;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Net;
using Tests.EndToEnd.Setup;
using Tests.EndToEnd.Setup.Modules.Catalog;
using Tests.EndToEnd.Setup.TestsHelpers;
using Xunit;

namespace Tests.EndToEnd.E2E_Catalog.Endpoints;
public sealed class AddProductTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task AddProduct_ShouldSucceed()
    {
        //Arrange
        var variationOption = Databases.CatalogDbContext.SeedVariationOption();
        var brand = Databases.CatalogDbContext.SeedBrand();
        var category = Databases.CatalogDbContext.SeedCategory();
        await Databases.CatalogDbContext.SaveChangesAsync();

        var name = Guid.NewGuid().ToString();

        var request = new AddProductCommand
        {
            BrandId = brand.Id.Value,
            CategoryId = category.Id.Value,
            VariationOptionIds = [variationOption.Id.Value],
            Name = name,
            Price = 100,
            Quantity = 5,
            Description = "sample description"
        };

        var tcs = RabbitMqChannel.CreateTestConsumerForPublishing<ProductAddedEvent>();

        //Act
        await TestContext.GuitarStoreClient.ProductsPOSTAsync(request);

        //Assert
        var product = await Databases.CatalogDbContext.Products.SingleOrDefaultAsync(x => x.Name == name);
        product.ShouldNotBeNull();
        product.Price.ShouldBe(100);
        product.Quantity.ShouldBe(5);

        var eventPublished = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        eventPublished.ShouldBeTrue();
    }

    [Fact]
    public async Task AddProduct_WhenCategoryNotExists_BadRequestReturned()
    {
        //Arrange
        var variationOption = Databases.CatalogDbContext.SeedVariationOption();
        var brand = Databases.CatalogDbContext.SeedBrand();
        await Databases.CatalogDbContext.SaveChangesAsync();

        var name = Guid.NewGuid().ToString();

        var request = new AddProductCommand
        {
            BrandId = brand.Id.Value,
            CategoryId = Guid.NewGuid(),
            VariationOptionIds = [variationOption.Id.Value],
            Name = name,
            Price = 100,
            Quantity = 5,
            Description = "sample description"
        };

        //Act
        var action = () => TestContext.GuitarStoreClient.ProductsPOSTAsync(request);

        //Assert
        var exception = await action.ShouldThrowAsync<ApiException>();
        var response = exception.ToFailedResponse();
        response.Status.ShouldBe((int)HttpStatusCode.Conflict);
    }
}
