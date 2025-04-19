using GuitarStore.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Tests.EndToEnd.Setup;
public class EndToEndTestBase : IAsyncLifetime, IAssemblyFixture<Application>
{
    protected readonly Application _webApp;
    protected IServiceScope Scope { get; private set; } = null!;
    //protected MrpDbContext Context => ServiceProvider.GetRequiredService<MrpDbContext>();

    protected TestContext TestContext { get; private set; } = null!;

    protected EndToEndTestBase(Application webApp)
    {
        _webApp = webApp;
    }

    public virtual Task InitializeAsync()
    {
        Scope = _webApp.ServiceProvider.CreateScope();
        TestContext = new(_webApp.GetAuthorizedClient());
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        Scope?.Dispose();
        return Task.CompletedTask;
    }
}

public sealed record TestContext(GuitarStoreClient GuitarStoreClient);
