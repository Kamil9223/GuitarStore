using GuitarStore.Api.Client;
using Shouldly;
using Tests.EndToEnd.Setup;
using Xunit;

namespace Tests.EndToEnd.E2E_Customers.Endpoints;
public sealed class CartDetailsTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task GetCartDetails_WhenCartDoesNotExists_NotFoundIsReturned()
    {
        //Act
        var action = () => TestContext.GuitarStoreClient.GetCartAsync();

        //Assert
        var ex = await Should.ThrowAsync<ApiException>(action);
        ex.StatusCode.ShouldBe(404);
    }
}
