using Tests.EndToEnd.Setup;
using Xunit;

namespace Tests.EndToEnd;
public sealed class InitialTest(Setup.Application app) : EndToEndTestBase(app)
{
    [Fact]
    public async Task Test()
    {
        var exception = await Record.ExceptionAsync(TestContext.GuitarStoreClient.TestAsync);

        Assert.Null(exception);
    }
}
